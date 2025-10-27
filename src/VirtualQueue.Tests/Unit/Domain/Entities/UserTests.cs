using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualQueue.Domain.Entities;
using VirtualQueue.Domain.Enums;

namespace VirtualQueue.Tests.Unit.Domain.Entities;

/// <summary>
/// Unit tests for the User entity.
/// </summary>
[TestClass]
public class UserTests
{
    #region Test Data
    private User _user;
    private Guid _tenantId;
    private const string ValidUsername = "testuser";
    private const string ValidEmail = "test@example.com";
    private const string ValidPasswordHash = "hashedpassword";
    private const string ValidFirstName = "Test";
    private const string ValidLastName = "User";
    #endregion

    #region Test Setup
    /// <summary>
    /// Sets up test data before each test.
    /// </summary>
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
    /// <summary>
    /// Tests that the constructor creates a user successfully with valid parameters.
    /// </summary>
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
        Assert.IsNotNull(user.Metadata);
        Assert.AreEqual(0, user.Metadata.Count);
    }

    /// <summary>
    /// Tests that the constructor throws an exception when tenant ID is empty.
    /// </summary>
    [TestMethod]
    public void Constructor_WithEmptyTenantId_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new User(Guid.Empty, ValidUsername, ValidEmail, ValidPasswordHash, ValidFirstName, ValidLastName));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when username is null.
    /// </summary>
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

    /// <summary>
    /// Tests that the constructor throws an exception when username is too short.
    /// </summary>
    [TestMethod]
    public void Constructor_WithShortUsername_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new User(_tenantId, "ab", ValidEmail, ValidPasswordHash, ValidFirstName, ValidLastName));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when username is too long.
    /// </summary>
    [TestMethod]
    public void Constructor_WithLongUsername_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var longUsername = new string('a', 51); // 51 characters
        Assert.ThrowsException<ArgumentException>(() =>
            new User(_tenantId, longUsername, ValidEmail, ValidPasswordHash, ValidFirstName, ValidLastName));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when email is invalid.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    [DataRow("invalid-email")]
    [DataRow("test@")]
    [DataRow("@example.com")]
    public void Constructor_WithInvalidEmail_ThrowsArgumentException(string email)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new User(_tenantId, ValidUsername, email, ValidPasswordHash, ValidFirstName, ValidLastName));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when password hash is null.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void Constructor_WithInvalidPasswordHash_ThrowsArgumentException(string passwordHash)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new User(_tenantId, ValidUsername, ValidEmail, passwordHash, ValidFirstName, ValidLastName));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when first name is null.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void Constructor_WithInvalidFirstName_ThrowsArgumentException(string firstName)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new User(_tenantId, ValidUsername, ValidEmail, ValidPasswordHash, firstName, ValidLastName));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when last name is null.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void Constructor_WithInvalidLastName_ThrowsArgumentException(string lastName)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new User(_tenantId, ValidUsername, ValidEmail, ValidPasswordHash, ValidFirstName, lastName));
    }
    #endregion

    #region UpdateProfile Tests
    /// <summary>
    /// Tests that UpdateProfile updates the profile successfully with valid data.
    /// </summary>
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

    /// <summary>
    /// Tests that UpdateProfile throws an exception when first name is null.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void UpdateProfile_WithInvalidFirstName_ThrowsArgumentException(string firstName)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            _user.UpdateProfile(firstName, ValidLastName));
    }

    /// <summary>
    /// Tests that UpdateProfile throws an exception when last name is null.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void UpdateProfile_WithInvalidLastName_ThrowsArgumentException(string lastName)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            _user.UpdateProfile(ValidFirstName, lastName));
    }

    /// <summary>
    /// Tests that UpdateProfile throws an exception when phone number is too long.
    /// </summary>
    [TestMethod]
    public void UpdateProfile_WithLongPhoneNumber_ThrowsArgumentException()
    {
        // Arrange
        var longPhoneNumber = new string('1', 21); // 21 characters

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            _user.UpdateProfile(ValidFirstName, ValidLastName, longPhoneNumber));
    }
    #endregion

    #region UpdateEmail Tests
    /// <summary>
    /// Tests that UpdateEmail updates the email successfully with valid email.
    /// </summary>
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

    /// <summary>
    /// Tests that UpdateEmail throws an exception when email is invalid.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    [DataRow("invalid-email")]
    [DataRow("test@")]
    [DataRow("@example.com")]
    public void UpdateEmail_WithInvalidEmail_ThrowsArgumentException(string email)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() => _user.UpdateEmail(email));
    }
    #endregion

    #region UpdatePassword Tests
    /// <summary>
    /// Tests that UpdatePassword updates the password successfully with valid password hash.
    /// </summary>
    [TestMethod]
    public void UpdatePassword_WithValidPasswordHash_UpdatesPasswordSuccessfully()
    {
        // Arrange
        var newPasswordHash = "newhashedpassword";

        // Act
        _user.UpdatePassword(newPasswordHash);

        // Assert
        Assert.AreEqual(newPasswordHash, _user.PasswordHash);
        Assert.IsNull(_user.RefreshToken);
        Assert.IsNull(_user.RefreshTokenExpiresAt);
    }

    /// <summary>
    /// Tests that UpdatePassword throws an exception when password hash is invalid.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void UpdatePassword_WithInvalidPasswordHash_ThrowsArgumentException(string passwordHash)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() => _user.UpdatePassword(passwordHash));
    }
    #endregion

    #region UpdateRole Tests
    /// <summary>
    /// Tests that UpdateRole updates the role successfully.
    /// </summary>
    [TestMethod]
    public void UpdateRole_WithValidRole_UpdatesRoleSuccessfully()
    {
        // Arrange
        var newRole = UserRole.Admin;

        // Act
        _user.UpdateRole(newRole);

        // Assert
        Assert.AreEqual(newRole, _user.Role);
    }

    /// <summary>
    /// Tests that UpdateRole does nothing when the role is the same.
    /// </summary>
    [TestMethod]
    public void UpdateRole_WithSameRole_DoesNothing()
    {
        // Arrange
        var originalRole = _user.Role;

        // Act
        _user.UpdateRole(originalRole);

        // Assert
        Assert.AreEqual(originalRole, _user.Role);
    }
    #endregion

    #region Status Management Tests
    /// <summary>
    /// Tests that Activate activates the user successfully.
    /// </summary>
    [TestMethod]
    public void Activate_WhenUserIsInactive_ActivatesUserSuccessfully()
    {
        // Arrange
        _user.Deactivate();
        Assert.AreEqual(UserStatus.Inactive, _user.Status);

        // Act
        _user.Activate();

        // Assert
        Assert.AreEqual(UserStatus.Active, _user.Status);
    }

    /// <summary>
    /// Tests that Deactivate deactivates the user successfully.
    /// </summary>
    [TestMethod]
    public void Deactivate_WhenUserIsActive_DeactivatesUserSuccessfully()
    {
        // Arrange
        Assert.AreEqual(UserStatus.Active, _user.Status);

        // Act
        _user.Deactivate();

        // Assert
        Assert.AreEqual(UserStatus.Inactive, _user.Status);
        Assert.IsNull(_user.RefreshToken);
        Assert.IsNull(_user.RefreshTokenExpiresAt);
    }

    /// <summary>
    /// Tests that Suspend suspends the user successfully.
    /// </summary>
    [TestMethod]
    public void Suspend_WithValidReason_SuspendsUserSuccessfully()
    {
        // Arrange
        var reason = "Violation of terms of service";

        // Act
        _user.Suspend(reason);

        // Assert
        Assert.AreEqual(UserStatus.Suspended, _user.Status);
        Assert.IsNull(_user.RefreshToken);
        Assert.IsNull(_user.RefreshTokenExpiresAt);
    }
    #endregion

    #region Login Tests
    /// <summary>
    /// Tests that RecordLogin records the login time successfully.
    /// </summary>
    [TestMethod]
    public void RecordLogin_RecordsLoginTimeSuccessfully()
    {
        // Arrange
        var beforeLogin = DateTime.UtcNow;

        // Act
        _user.RecordLogin();

        // Assert
        Assert.IsNotNull(_user.LastLoginAt);
        Assert.IsTrue(_user.LastLoginAt.Value >= beforeLogin);
    }
    #endregion

    #region Verification Tests
    /// <summary>
    /// Tests that VerifyEmail verifies the email successfully.
    /// </summary>
    [TestMethod]
    public void VerifyEmail_VerifiesEmailSuccessfully()
    {
        // Arrange
        Assert.IsNull(_user.EmailVerifiedAt);

        // Act
        _user.VerifyEmail();

        // Assert
        Assert.IsNotNull(_user.EmailVerifiedAt);
        Assert.IsTrue(_user.IsEmailVerified());
    }

    /// <summary>
    /// Tests that VerifyPhone verifies the phone successfully.
    /// </summary>
    [TestMethod]
    public void VerifyPhone_VerifiesPhoneSuccessfully()
    {
        // Arrange
        _user.UpdateProfile(_user.FirstName, _user.LastName, "1234567890");
        Assert.IsNull(_user.PhoneVerifiedAt);

        // Act
        _user.VerifyPhone();

        // Assert
        Assert.IsNotNull(_user.PhoneVerifiedAt);
        Assert.IsTrue(_user.IsPhoneVerified());
    }
    #endregion

    #region Two-Factor Authentication Tests
    /// <summary>
    /// Tests that EnableTwoFactor enables two-factor authentication successfully.
    /// </summary>
    [TestMethod]
    public void EnableTwoFactor_WithValidSecret_EnablesTwoFactorSuccessfully()
    {
        // Arrange
        var secret = "testsecret";

        // Act
        _user.EnableTwoFactor(secret);

        // Assert
        Assert.AreEqual(secret, _user.TwoFactorSecret);
        Assert.IsTrue(_user.IsTwoFactorEnabled);
    }

    /// <summary>
    /// Tests that EnableTwoFactor throws an exception when secret is invalid.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void EnableTwoFactor_WithInvalidSecret_ThrowsArgumentException(string secret)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() => _user.EnableTwoFactor(secret));
    }

    /// <summary>
    /// Tests that DisableTwoFactor disables two-factor authentication successfully.
    /// </summary>
    [TestMethod]
    public void DisableTwoFactor_DisablesTwoFactorSuccessfully()
    {
        // Arrange
        _user.EnableTwoFactor("testsecret");
        Assert.IsTrue(_user.IsTwoFactorEnabled);

        // Act
        _user.DisableTwoFactor();

        // Assert
        Assert.IsNull(_user.TwoFactorSecret);
        Assert.IsFalse(_user.IsTwoFactorEnabled);
    }
    #endregion

    #region Refresh Token Tests
    /// <summary>
    /// Tests that SetRefreshToken sets the refresh token successfully.
    /// </summary>
    [TestMethod]
    public void SetRefreshToken_WithValidToken_SetsRefreshTokenSuccessfully()
    {
        // Arrange
        var refreshToken = "testrefreshtoken";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        _user.SetRefreshToken(refreshToken, expiresAt);

        // Assert
        Assert.AreEqual(refreshToken, _user.RefreshToken);
        Assert.AreEqual(expiresAt, _user.RefreshTokenExpiresAt);
        Assert.IsTrue(_user.HasValidRefreshToken());
    }

    /// <summary>
    /// Tests that RevokeRefreshToken revokes the refresh token successfully.
    /// </summary>
    [TestMethod]
    public void RevokeRefreshToken_RevokesRefreshTokenSuccessfully()
    {
        // Arrange
        _user.SetRefreshToken("testtoken", DateTime.UtcNow.AddDays(7));
        Assert.IsNotNull(_user.RefreshToken);

        // Act
        _user.RevokeRefreshToken();

        // Assert
        Assert.IsNull(_user.RefreshToken);
        Assert.IsNull(_user.RefreshTokenExpiresAt);
        Assert.IsFalse(_user.HasValidRefreshToken());
    }
    #endregion

    #region Metadata Tests
    /// <summary>
    /// Tests that UpdateMetadata updates metadata successfully.
    /// </summary>
    [TestMethod]
    public void UpdateMetadata_WithValidKeyAndValue_UpdatesMetadataSuccessfully()
    {
        // Arrange
        var key = "testkey";
        var value = "testvalue";

        // Act
        _user.UpdateMetadata(key, value);

        // Assert
        Assert.IsTrue(_user.Metadata.ContainsKey(key));
        Assert.AreEqual(value, _user.Metadata[key]);
    }

    /// <summary>
    /// Tests that UpdateMetadata throws an exception when key is invalid.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void UpdateMetadata_WithInvalidKey_ThrowsArgumentException(string key)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() => _user.UpdateMetadata(key, "value"));
    }

    /// <summary>
    /// Tests that RemoveMetadata removes metadata successfully.
    /// </summary>
    [TestMethod]
    public void RemoveMetadata_WithValidKey_RemovesMetadataSuccessfully()
    {
        // Arrange
        var key = "testkey";
        _user.UpdateMetadata(key, "testvalue");
        Assert.IsTrue(_user.Metadata.ContainsKey(key));

        // Act
        _user.RemoveMetadata(key);

        // Assert
        Assert.IsFalse(_user.Metadata.ContainsKey(key));
    }
    #endregion

    #region Utility Method Tests
    /// <summary>
    /// Tests that GetFullName returns the correct full name.
    /// </summary>
    [TestMethod]
    public void GetFullName_ReturnsCorrectFullName()
    {
        // Arrange
        var expectedFullName = $"{ValidFirstName} {ValidLastName}";

        // Act
        var actualFullName = _user.GetFullName();

        // Assert
        Assert.AreEqual(expectedFullName, actualFullName);
    }

    /// <summary>
    /// Tests that CanLogin returns true when user is active.
    /// </summary>
    [TestMethod]
    public void CanLogin_WhenUserIsActive_ReturnsTrue()
    {
        // Arrange
        Assert.AreEqual(UserStatus.Active, _user.Status);

        // Act
        var canLogin = _user.CanLogin();

        // Assert
        Assert.IsTrue(canLogin);
    }

    /// <summary>
    /// Tests that CanLogin returns false when user is inactive.
    /// </summary>
    [TestMethod]
    public void CanLogin_WhenUserIsInactive_ReturnsFalse()
    {
        // Arrange
        _user.Deactivate();

        // Act
        var canLogin = _user.CanLogin();

        // Assert
        Assert.IsFalse(canLogin);
    }
    #endregion
}
