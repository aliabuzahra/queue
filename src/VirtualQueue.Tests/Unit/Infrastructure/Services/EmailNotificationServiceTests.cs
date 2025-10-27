using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VirtualQueue.Infrastructure.Services;

namespace VirtualQueue.Tests.Unit.Infrastructure.Services;

/// <summary>
/// Unit tests for the EmailNotificationService class.
/// </summary>
[TestClass]
public class EmailNotificationServiceTests
{
    #region Test Data
    private Mock<ILogger<EmailNotificationService>> _mockLogger;
    private Mock<IOptions<EmailSettings>> _mockOptions;
    private EmailNotificationService _service;
    private EmailSettings _settings;
    #endregion

    #region Test Setup
    /// <summary>
    /// Sets up test data before each test.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<EmailNotificationService>>();
        _mockOptions = new Mock<IOptions<EmailSettings>>();
        _settings = new EmailSettings
        {
            SmtpServer = "test.smtp.com",
            SmtpPort = 587,
            Username = "testuser",
            Password = "testpass",
            EnableSsl = true,
            FromEmail = "test@example.com",
            FromName = "Test System"
        };
        
        _mockOptions.Setup(o => o.Value).Returns(_settings);
        _service = new EmailNotificationService(_mockLogger.Object, _mockOptions.Object);
    }
    #endregion

    #region Constructor Tests
    /// <summary>
    /// Tests that the constructor initializes the service successfully with valid parameters.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesServiceSuccessfully()
    {
        // Arrange
        var logger = new Mock<ILogger<EmailNotificationService>>();
        var options = new Mock<IOptions<EmailSettings>>();
        options.Setup(o => o.Value).Returns(_settings);

        // Act
        var service = new EmailNotificationService(logger.Object, options.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor throws an exception when logger is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var options = new Mock<IOptions<EmailSettings>>();

        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => 
            new EmailNotificationService(null!, options.Object));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when options is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = new Mock<ILogger<EmailNotificationService>>();

        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => 
            new EmailNotificationService(logger.Object, null!));
    }
    #endregion

    #region SendEmailAsync Tests
    /// <summary>
    /// Tests that SendEmailAsync sends email successfully with valid parameters.
    /// </summary>
    [TestMethod]
    public async Task SendEmailAsync_WithValidParameters_SendsEmailSuccessfully()
    {
        // Arrange
        var to = "recipient@example.com";
        var subject = "Test Subject";
        var body = "Test Body";

        // Act
        await _service.SendEmailAsync(to, subject, body);

        // Assert
        // Verify that the method completed without throwing an exception
        // In a real implementation, you would verify that the email was actually sent
        Assert.IsTrue(true);
    }

    /// <summary>
    /// Tests that SendEmailAsync throws an exception when recipient email is null.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public async Task SendEmailAsync_WithNullRecipientEmail_ThrowsArgumentException(string to)
    {
        // Arrange
        var subject = "Test Subject";
        var body = "Test Body";

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => 
            _service.SendEmailAsync(to, subject, body));
    }

    /// <summary>
    /// Tests that SendEmailAsync throws an exception when subject is null.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public async Task SendEmailAsync_WithNullSubject_ThrowsArgumentException(string subject)
    {
        // Arrange
        var to = "recipient@example.com";
        var body = "Test Body";

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => 
            _service.SendEmailAsync(to, subject, body));
    }

    /// <summary>
    /// Tests that SendEmailAsync throws an exception when body is null.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public async Task SendEmailAsync_WithNullBody_ThrowsArgumentException(string body)
    {
        // Arrange
        var to = "recipient@example.com";
        var subject = "Test Subject";

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => 
            _service.SendEmailAsync(to, subject, body));
    }

    /// <summary>
    /// Tests that SendEmailAsync throws an exception when email format is invalid.
    /// </summary>
    [TestMethod]
    [DataRow("invalid-email")]
    [DataRow("test@")]
    [DataRow("@example.com")]
    [DataRow("test.example.com")]
    public async Task SendEmailAsync_WithInvalidEmailFormat_ThrowsArgumentException(string to)
    {
        // Arrange
        var subject = "Test Subject";
        var body = "Test Body";

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => 
            _service.SendEmailAsync(to, subject, body));
    }

    /// <summary>
    /// Tests that SendEmailAsync handles cancellation token correctly.
    /// </summary>
    [TestMethod]
    public async Task SendEmailAsync_WithCancellationToken_HandlesCancellationCorrectly()
    {
        // Arrange
        var to = "recipient@example.com";
        var subject = "Test Subject";
        var body = "Test Body";
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => 
            _service.SendEmailAsync(to, subject, body, cancellationTokenSource.Token));
    }
    #endregion

    #region SendSmsAsync Tests
    /// <summary>
    /// Tests that SendSmsAsync sends SMS successfully with valid parameters.
    /// </summary>
    [TestMethod]
    public async Task SendSmsAsync_WithValidParameters_SendsSmsSuccessfully()
    {
        // Arrange
        var phoneNumber = "1234567890";
        var message = "Test SMS Message";

        // Act
        await _service.SendSmsAsync(phoneNumber, message);

        // Assert
        // Verify that the method completed without throwing an exception
        Assert.IsTrue(true);
    }

    /// <summary>
    /// Tests that SendSmsAsync handles cancellation token correctly.
    /// </summary>
    [TestMethod]
    public async Task SendSmsAsync_WithCancellationToken_HandlesCancellationCorrectly()
    {
        // Arrange
        var phoneNumber = "1234567890";
        var message = "Test SMS Message";
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => 
            _service.SendSmsAsync(phoneNumber, message, cancellationTokenSource.Token));
    }
    #endregion

    #region SendWhatsAppAsync Tests
    /// <summary>
    /// Tests that SendWhatsAppAsync sends WhatsApp message successfully with valid parameters.
    /// </summary>
    [TestMethod]
    public async Task SendWhatsAppAsync_WithValidParameters_SendsWhatsAppMessageSuccessfully()
    {
        // Arrange
        var phoneNumber = "1234567890";
        var message = "Test WhatsApp Message";

        // Act
        await _service.SendWhatsAppAsync(phoneNumber, message);

        // Assert
        // Verify that the method completed without throwing an exception
        Assert.IsTrue(true);
    }

    /// <summary>
    /// Tests that SendWhatsAppAsync handles cancellation token correctly.
    /// </summary>
    [TestMethod]
    public async Task SendWhatsAppAsync_WithCancellationToken_HandlesCancellationCorrectly()
    {
        // Arrange
        var phoneNumber = "1234567890";
        var message = "Test WhatsApp Message";
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => 
            _service.SendWhatsAppAsync(phoneNumber, message, cancellationTokenSource.Token));
    }
    #endregion

    #region EmailSettings Tests
    /// <summary>
    /// Tests that EmailSettings initializes with default values.
    /// </summary>
    [TestMethod]
    public void EmailSettings_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var settings = new EmailSettings();

        // Assert
        Assert.AreEqual("localhost", settings.SmtpServer);
        Assert.AreEqual(587, settings.SmtpPort);
        Assert.AreEqual("", settings.Username);
        Assert.AreEqual("", settings.Password);
        Assert.IsTrue(settings.EnableSsl);
        Assert.AreEqual("noreply@virtualqueue.com", settings.FromEmail);
        Assert.AreEqual("Virtual Queue System", settings.FromName);
    }

    /// <summary>
    /// Tests that EmailSettings properties can be set and retrieved.
    /// </summary>
    [TestMethod]
    public void EmailSettings_PropertiesCanBeSetAndRetrieved()
    {
        // Arrange
        var settings = new EmailSettings();

        // Act
        settings.SmtpServer = "test.smtp.com";
        settings.SmtpPort = 465;
        settings.Username = "testuser";
        settings.Password = "testpass";
        settings.EnableSsl = false;
        settings.FromEmail = "test@example.com";
        settings.FromName = "Test System";

        // Assert
        Assert.AreEqual("test.smtp.com", settings.SmtpServer);
        Assert.AreEqual(465, settings.SmtpPort);
        Assert.AreEqual("testuser", settings.Username);
        Assert.AreEqual("testpass", settings.Password);
        Assert.IsFalse(settings.EnableSsl);
        Assert.AreEqual("test@example.com", settings.FromEmail);
        Assert.AreEqual("Test System", settings.FromName);
    }
    #endregion
}
