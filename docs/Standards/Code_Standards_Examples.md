# Code Standards Examples - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** Technical Lead  
**Status:** Approved  
**Phase:** 2 - Requirements & Architecture  
**Priority:** 🔴 Critical  

---

## Executive Summary

This document provides practical examples of how to apply coding standards to the Virtual Queue Management System. It includes before/after comparisons and best practice implementations.

## Code Standards Examples

### **1. Entity Design - Domain Entity**

#### **❌ Poor Example**
```csharp
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public void UpdateEmail(string email)
    {
        Email = email;
    }
}
```

#### **✅ Good Example**
```csharp
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
    #endregion

    #region Constructors
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
    /// Thrown when any of the required parameters are null or empty.
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
    #endregion

    #region Public Methods
    /// <summary>
    /// Updates the user's email address.
    /// </summary>
    /// <param name="email">The new email address.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the email is null, empty, or invalid.
    /// </exception>
    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
            
        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        var oldEmail = Email;
        Email = email;
        EmailVerifiedAt = null; // Require re-verification

        AddDomainEvent(new UserEmailUpdatedEvent(Id, TenantId, Username, oldEmail, email));
    }
    #endregion

    #region Private Methods
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
            
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));
            
        if (username.Length < MinUsernameLength)
            throw new ArgumentException($"Username must be at least {MinUsernameLength} characters long", nameof(username));
            
        if (username.Length > MaxUsernameLength)
            throw new ArgumentException($"Username cannot exceed {MaxUsernameLength} characters", nameof(username));
            
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
            
        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));
            
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be null or empty", nameof(passwordHash));
            
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty", nameof(firstName));
            
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));
    }
    
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
```

### **2. Service Implementation - Infrastructure Service**

#### **❌ Poor Example**
```csharp
public class EmailService
{
    public async Task SendEmail(string to, string subject, string body)
    {
        // Send email logic
    }
}
```

#### **✅ Good Example**
```csharp
/// <summary>
/// Service for sending email notifications.
/// </summary>
public class EmailNotificationService : INotificationService
{
    #region Fields
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly EmailSettings _settings;
    private readonly ISmtpClient _smtpClient;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailNotificationService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="settings">The email settings.</param>
    /// <param name="smtpClient">The SMTP client.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of the required parameters are null.
    /// </exception>
    public EmailNotificationService(
        ILogger<EmailNotificationService> logger,
        IOptions<EmailSettings> settings,
        ISmtpClient smtpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _smtpClient = smtpClient ?? throw new ArgumentNullException(nameof(smtpClient));
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Sends an email notification asynchronously.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="body">The email body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of the required parameters are null or empty.
    /// </exception>
    /// <exception cref="NotificationException">
    /// Thrown when the email cannot be sent.
    /// </exception>
    public async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        ValidateEmailParameters(to, subject, body);
        
        try
        {
            _logger.LogInformation("Sending email to {Email} with subject: {Subject}", to, subject);
            
            var message = CreateEmailMessage(to, subject, body);
            await _smtpClient.SendAsync(message, cancellationToken);
            
            _logger.LogInformation("Email sent successfully to {Email}", to);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error sending email to {Email}: {Error}", to, ex.Message);
            throw new NotificationException($"Failed to send email to {to}: {ex.Message}", ex);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout sending email to {Email}", to);
            throw new NotificationException($"Email service timeout for {to}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending email to {Email}", to);
            throw new NotificationException($"Unexpected error sending email to {to}", ex);
        }
    }
    #endregion

    #region Private Methods
    private static void ValidateEmailParameters(string to, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Recipient email cannot be null or empty", nameof(to));
            
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Email subject cannot be null or empty", nameof(subject));
            
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Email body cannot be null or empty", nameof(body));
            
        if (!IsValidEmail(to))
            throw new ArgumentException("Invalid email format", nameof(to));
    }
    
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
    
    private MailMessage CreateEmailMessage(string to, string subject, string body)
    {
        return new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            To = { new MailAddress(to) },
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
    }
    #endregion
}
```

### **3. Repository Implementation - Data Access**

#### **❌ Poor Example**
```csharp
public class UserRepository
{
    public User GetUser(int id)
    {
        // Database logic
    }
}
```

#### **✅ Good Example**
```csharp
/// <summary>
/// Repository for managing user data access operations.
/// </summary>
public class UserRepository : BaseRepository<User>, IUserRepository
{
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the context is null.
    /// </exception>
    public UserRepository(VirtualQueueDbContext context) : base(context)
    {
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Retrieves a user by their username within a specific tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="username">The username.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The user if found; otherwise, null.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when tenantId is empty or username is null or empty.
    /// </exception>
    public async Task<User?> GetByUsernameAsync(
        Guid tenantId,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));

        return await _dbSet
            .FirstOrDefaultAsync(
                u => u.TenantId == tenantId && u.Username == username,
                cancellationToken);
    }

    /// <summary>
    /// Retrieves all users for a specific tenant with pagination.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A paginated list of users.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when tenantId is empty or pagination parameters are invalid.
    /// </exception>
    public async Task<PaginatedResult<User>> GetByTenantAsync(
        Guid tenantId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));
            
        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("Page size must be between 1 and 100", nameof(pageSize));

        var query = _dbSet.Where(u => u.TenantId == tenantId);
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var users = await query
            .OrderBy(u => u.Username)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<User>
        {
            Items = users,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }
    #endregion
}
```

### **4. API Controller - Web API**

#### **❌ Poor Example**
```csharp
[ApiController]
public class UsersController : ControllerBase
{
    public IActionResult GetUser(int id)
    {
        // Controller logic
    }
}
```

#### **✅ Good Example**
```csharp
/// <summary>
/// Controller for managing user operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    #region Fields
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator for handling commands and queries.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of the required parameters are null.
    /// </exception>
    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The user information if found; otherwise, a 404 Not Found response.
    /// </returns>
    /// <response code="200">Returns the user information.</response>
    /// <response code="404">User not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> GetUserAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving user with ID: {UserId}", id);
            
            var query = new GetUserByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);
            
            if (result == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", id);
                return NotFound($"User with ID {id} not found");
            }
            
            _logger.LogInformation("Successfully retrieved user with ID: {UserId}", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the user");
        }
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="request">The user creation request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The created user information.
    /// </returns>
    /// <response code="201">User created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> CreateUserAsync(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating user with username: {Username}", request.Username);
            
            var command = new CreateUserCommand(
                request.TenantId,
                request.Username,
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.Role);
                
            var result = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Successfully created user with ID: {UserId}", result.Id);
            
            return CreatedAtAction(
                nameof(GetUserAsync),
                new { id = result.Id },
                result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error creating user: {Errors}", string.Join(", ", ex.Errors));
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with username: {Username}", request.Username);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the user");
        }
    }
    #endregion
}
```

### **5. Unit Test - Testing Standards**

#### **❌ Poor Example**
```csharp
[Test]
public void TestUser()
{
    var user = new User();
    user.Username = "test";
    Assert.IsTrue(user.Username == "test");
}
```

#### **✅ Good Example**
```csharp
/// <summary>
/// Unit tests for the User entity.
/// </summary>
[TestClass]
public class UserTests
{
    #region Fields
    private User _user;
    private Guid _tenantId;
    private const string ValidUsername = "testuser";
    private const string ValidEmail = "test@example.com";
    private const string ValidPasswordHash = "hashedpassword";
    private const string ValidFirstName = "Test";
    private const string ValidLastName = "User";
    #endregion

    #region Test Setup
    [TestInitialize]
    public void Setup()
    {
        _tenantId = Guid.NewGuid();
        _user = new User(
            _tenantId,
            ValidUsername,
            ValidEmail,
            ValidPasswordHash,
            ValidFirstName,
            ValidLastName);
    }
    #endregion

    #region Constructor Tests
    [TestMethod]
    public void Constructor_WithValidParameters_CreatesUserSuccessfully()
    {
        // Arrange & Act
        var user = new User(
            _tenantId,
            ValidUsername,
            ValidEmail,
            ValidPasswordHash,
            ValidFirstName,
            ValidLastName);

        // Assert
        Assert.IsNotNull(user);
        Assert.AreEqual(_tenantId, user.TenantId);
        Assert.AreEqual(ValidUsername, user.Username);
        Assert.AreEqual(ValidEmail, user.Email);
        Assert.AreEqual(ValidPasswordHash, user.PasswordHash);
        Assert.AreEqual(ValidFirstName, user.FirstName);
        Assert.AreEqual(ValidLastName, user.LastName);
        Assert.AreEqual(UserRole.User, user.Role);
        Assert.AreEqual(UserStatus.Active, user.Status);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void Constructor_WithInvalidUsername_ThrowsArgumentException(string username)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new User(_tenantId, username, ValidEmail, ValidPasswordHash, ValidFirstName, ValidLastName));
    }

    [TestMethod]
    public void Constructor_WithEmptyTenantId_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new User(Guid.Empty, ValidUsername, ValidEmail, ValidPasswordHash, ValidFirstName, ValidLastName));
    }
    #endregion

    #region UpdateEmail Tests
    [TestMethod]
    public void UpdateEmail_WithValidEmail_UpdatesEmailSuccessfully()
    {
        // Arrange
        var newEmail = "newemail@example.com";

        // Act
        _user.UpdateEmail(newEmail);

        // Assert
        Assert.AreEqual(newEmail, _user.Email);
        Assert.IsNull(_user.EmailVerifiedAt);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    [DataRow("invalid-email")]
    public void UpdateEmail_WithInvalidEmail_ThrowsArgumentException(string email)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() => _user.UpdateEmail(email));
    }
    #endregion

    #region UpdateProfile Tests
    [TestMethod]
    public void UpdateProfile_WithValidData_UpdatesProfileSuccessfully()
    {
        // Arrange
        var newFirstName = "NewFirst";
        var newLastName = "NewLast";
        var newPhoneNumber = "1234567890";
        var newProfileImageUrl = "https://example.com/image.jpg";

        // Act
        _user.UpdateProfile(newFirstName, newLastName, newPhoneNumber, newProfileImageUrl);

        // Assert
        Assert.AreEqual(newFirstName, _user.FirstName);
        Assert.AreEqual(newLastName, _user.LastName);
        Assert.AreEqual(newPhoneNumber, _user.PhoneNumber);
        Assert.AreEqual(newProfileImageUrl, _user.ProfileImageUrl);
    }

    [TestMethod]
    public void UpdateProfile_WithEmptyFirstName_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            _user.UpdateProfile("", ValidLastName));
    }
    #endregion
}
```

## Summary

These examples demonstrate how to apply coding standards to create maintainable, readable, and robust code. The key principles are:

1. **Comprehensive Documentation** - XML documentation for all public APIs
2. **Input Validation** - Validate all inputs with meaningful error messages
3. **Error Handling** - Specific exception types and proper error handling
4. **Code Organization** - Logical grouping of members and methods
5. **Testing** - Comprehensive unit tests with clear naming and structure
6. **SOLID Principles** - Single responsibility, dependency injection, and proper abstractions

## Approval

### **Code Standards Examples Approval**
- **Technical Lead**: [Name] - [Date]
- **Senior Developer**: [Name] - [Date]
- **Quality Assurance**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: Development Team

---

**Document Status**: Approved  
**Next Phase**: Code Quality Implementation  
**Dependencies**: Development team training, code review process implementation
