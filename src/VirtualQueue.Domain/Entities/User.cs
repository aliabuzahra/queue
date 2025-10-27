using VirtualQueue.Domain.Common;
using VirtualQueue.Domain.Enums;
using VirtualQueue.Domain.Events;

namespace VirtualQueue.Domain.Entities;

/// <summary>
/// Represents a user in the virtual queue system.
/// </summary>
/// <remarks>
/// This class encapsulates user information and provides
/// methods for managing user state in the queue.
/// </remarks>
public class User : BaseEntity
{
    #region Constants
    private const int MinUsernameLength = 3;
    private const int MaxUsernameLength = 50;
    private const int MinPasswordLength = 8;
    private const int MaxEmailLength = 255;
    private const int MaxPhoneLength = 20;
    private const int MaxFirstNameLength = 50;
    private const int MaxLastNameLength = 50;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the tenant identifier for this user.
    /// </summary>
    public Guid TenantId { get; private set; }
    
    /// <summary>
    /// Gets the username for this user.
    /// </summary>
    public string Username { get; private set; }
    
    /// <summary>
    /// Gets the email address for this user.
    /// </summary>
    public string Email { get; private set; }
    
    /// <summary>
    /// Gets the hashed password for this user.
    /// </summary>
    public string PasswordHash { get; private set; }
    
    /// <summary>
    /// Gets the first name of this user.
    /// </summary>
    public string FirstName { get; private set; }
    
    /// <summary>
    /// Gets the last name of this user.
    /// </summary>
    public string LastName { get; private set; }
    
    /// <summary>
    /// Gets the phone number of this user.
    /// </summary>
    public string? PhoneNumber { get; private set; }
    
    /// <summary>
    /// Gets the role of this user.
    /// </summary>
    public UserRole Role { get; private set; }
    
    /// <summary>
    /// Gets the status of this user.
    /// </summary>
    public UserStatus Status { get; private set; }
    
    /// <summary>
    /// Gets the date and time of the last login.
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }
    
    /// <summary>
    /// Gets the date and time when the email was verified.
    /// </summary>
    public DateTime? EmailVerifiedAt { get; private set; }
    
    /// <summary>
    /// Gets the date and time when the phone was verified.
    /// </summary>
    public DateTime? PhoneVerifiedAt { get; private set; }
    
    /// <summary>
    /// Gets the two-factor authentication secret.
    /// </summary>
    public string? TwoFactorSecret { get; private set; }
    
    /// <summary>
    /// Gets a value indicating whether two-factor authentication is enabled.
    /// </summary>
    public bool IsTwoFactorEnabled { get; private set; }
    
    /// <summary>
    /// Gets the refresh token for this user.
    /// </summary>
    public string? RefreshToken { get; private set; }
    
    /// <summary>
    /// Gets the date and time when the refresh token expires.
    /// </summary>
    public DateTime? RefreshTokenExpiresAt { get; private set; }
    
    /// <summary>
    /// Gets the profile image URL for this user.
    /// </summary>
    public string? ProfileImageUrl { get; private set; }
    
    /// <summary>
    /// Gets the metadata for this user.
    /// </summary>
    public Dictionary<string, string> Metadata { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// This constructor is used by Entity Framework Core.
    /// </summary>
    private User() { }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="username">The username.</param>
    /// <param name="email">The email address.</param>
    /// <param name="passwordHash">The hashed password.</param>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name.</param>
    /// <param name="role">The user role.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when any of the required parameters are null, empty, or invalid.
    /// </exception>
    public User(
        Guid tenantId,
        string username,
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        UserRole role = UserRole.User)
    {
        ValidateInputs(tenantId, username, email, passwordHash, firstName, lastName);

        TenantId = tenantId;
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        Role = role;
        Status = UserStatus.Active;
        Metadata = new Dictionary<string, string>();

        AddDomainEvent(new UserCreatedEvent(Id, TenantId, Username, Email));
    }

    #region Public Methods
    /// <summary>
    /// Updates the user's profile information.
    /// </summary>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name.</param>
    /// <param name="phoneNumber">The phone number (optional).</param>
    /// <param name="profileImageUrl">The profile image URL (optional).</param>
    /// <exception cref="ArgumentException">
    /// Thrown when firstName or lastName are null, empty, or exceed maximum length.
    /// </exception>
    public void UpdateProfile(string firstName, string lastName, string? phoneNumber = null, string? profileImageUrl = null)
    {
        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));
        
        if (phoneNumber != null && phoneNumber.Length > MaxPhoneLength)
            throw new ArgumentException($"Phone number cannot exceed {MaxPhoneLength} characters", nameof(phoneNumber));

        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        ProfileImageUrl = profileImageUrl;

        AddDomainEvent(new UserProfileUpdatedEvent(Id, TenantId, Username));
    }

    /// <summary>
    /// Updates the user's email address and marks it as unverified.
    /// </summary>
    /// <param name="email">The new email address.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the email is null, empty, or invalid.
    /// </exception>
    public void UpdateEmail(string email)
    {
        ValidateEmail(email);

        var oldEmail = Email;
        Email = email;
        EmailVerifiedAt = null; // Require re-verification

        AddDomainEvent(new UserEmailUpdatedEvent(Id, TenantId, Username, oldEmail, email));
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be null or empty", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        RefreshToken = null; // Invalidate refresh token
        RefreshTokenExpiresAt = null;

        AddDomainEvent(new UserPasswordUpdatedEvent(Id, TenantId, Username));
    }

    public void UpdateRole(UserRole newRole)
    {
        if (Role == newRole) return;

        var oldRole = Role;
        Role = newRole;

        AddDomainEvent(new UserRoleUpdatedEvent(Id, TenantId, Username, oldRole.ToString(), newRole.ToString()));
    }

    public void Activate()
    {
        if (Status == UserStatus.Active) return;

        Status = UserStatus.Active;
        AddDomainEvent(new UserActivatedEvent(Id, TenantId, Username));
    }

    public void Deactivate()
    {
        if (Status == UserStatus.Inactive) return;

        Status = UserStatus.Inactive;
        RefreshToken = null; // Invalidate refresh token
        RefreshTokenExpiresAt = null;

        AddDomainEvent(new UserDeactivatedEvent(Id, TenantId, Username));
    }

    public void Suspend(string reason)
    {
        if (Status == UserStatus.Suspended) return;

        Status = UserStatus.Suspended;
        RefreshToken = null; // Invalidate refresh token
        RefreshTokenExpiresAt = null;

        AddDomainEvent(new UserSuspendedEvent(Id, TenantId, Username, reason));
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        AddDomainEvent(new UserLoggedInEvent(Id, TenantId, Username, LastLoginAt.Value));
    }

    public void VerifyEmail()
    {
        EmailVerifiedAt = DateTime.UtcNow;
        AddDomainEvent(new UserEmailVerifiedEvent(Id, TenantId, Username, Email));
    }

    public void VerifyPhone()
    {
        PhoneVerifiedAt = DateTime.UtcNow;
        AddDomainEvent(new UserPhoneVerifiedEvent(Id, TenantId, Username, PhoneNumber!));
    }

    public void EnableTwoFactor(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("Two-factor secret cannot be null or empty", nameof(secret));

        TwoFactorSecret = secret;
        IsTwoFactorEnabled = true;

        AddDomainEvent(new UserTwoFactorEnabledEvent(Id, TenantId, Username));
    }

    public void DisableTwoFactor()
    {
        TwoFactorSecret = null;
        IsTwoFactorEnabled = false;

        AddDomainEvent(new UserTwoFactorDisabledEvent(Id, TenantId, Username));
    }

    public void SetRefreshToken(string refreshToken, DateTime expiresAt)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiresAt = expiresAt;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
    }

    public void UpdateMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty", nameof(key));

        Metadata[key] = value;
    }

    public void RemoveMetadata(string key)
    {
        if (Metadata.ContainsKey(key))
        {
            Metadata.Remove(key);
        }
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    public bool IsEmailVerified() => EmailVerifiedAt.HasValue;

    public bool IsPhoneVerified() => PhoneVerifiedAt.HasValue;

    public bool HasValidRefreshToken() => 
        !string.IsNullOrEmpty(RefreshToken) && 
        RefreshTokenExpiresAt.HasValue && 
        RefreshTokenExpiresAt.Value > DateTime.UtcNow;

    public bool CanLogin() => Status == UserStatus.Active;
    #endregion

    #region Private Methods
    /// <summary>
    /// Validates all input parameters for the constructor.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="username">The username.</param>
    /// <param name="email">The email address.</param>
    /// <param name="passwordHash">The password hash.</param>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when any parameter is invalid.
    /// </exception>
    private static void ValidateInputs(
        Guid tenantId,
        string username,
        string email,
        string passwordHash,
        string firstName,
        string lastName)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            
        ValidateUsername(username);
        ValidateEmail(email);
        ValidatePasswordHash(passwordHash);
        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));
    }

    /// <summary>
    /// Validates a username.
    /// </summary>
    /// <param name="username">The username to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the username is invalid.
    /// </exception>
    private static void ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));
            
        if (username.Length < MinUsernameLength)
            throw new ArgumentException($"Username must be at least {MinUsernameLength} characters long", nameof(username));
            
        if (username.Length > MaxUsernameLength)
            throw new ArgumentException($"Username cannot exceed {MaxUsernameLength} characters", nameof(username));
    }

    /// <summary>
    /// Validates an email address.
    /// </summary>
    /// <param name="email">The email to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the email is invalid.
    /// </exception>
    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
            
        if (email.Length > MaxEmailLength)
            throw new ArgumentException($"Email cannot exceed {MaxEmailLength} characters", nameof(email));
            
        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));
    }

    /// <summary>
    /// Validates a password hash.
    /// </summary>
    /// <param name="passwordHash">The password hash to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the password hash is invalid.
    /// </exception>
    private static void ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be null or empty", nameof(passwordHash));
    }

    /// <summary>
    /// Validates a name (first name or last name).
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <param name="paramName">The parameter name for error messages.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the name is invalid.
    /// </exception>
    private static void ValidateName(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
            
        if (name.Length > MaxFirstNameLength)
            throw new ArgumentException($"{paramName} cannot exceed {MaxFirstNameLength} characters", paramName);
    }

    /// <summary>
    /// Validates an email address format.
    /// </summary>
    /// <param name="email">The email to validate.</param>
    /// <returns>True if the email is valid; otherwise, false.</returns>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    #endregion
}
