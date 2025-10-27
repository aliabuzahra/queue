using Microsoft.Extensions.Logging;
using Moq;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Infrastructure.Services;
using FluentAssertions;

namespace VirtualQueue.Tests.Services;

/// <summary>
/// Unit tests for the EmailNotificationService
/// </summary>
public class EmailNotificationServiceTests
{
    #region Fields
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<EmailNotificationService>> _mockLogger;
    private readonly EmailNotificationService _emailService;
    #endregion

    #region Constructor
    public EmailNotificationServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<EmailNotificationService>>();

        // Setup configuration
        _mockConfiguration.Setup(x => x["Email:SmtpServer"]).Returns("smtp.example.com");
        _mockConfiguration.Setup(x => x["Email:SmtpPort"]).Returns("587");
        _mockConfiguration.Setup(x => x["Email:Username"]).Returns("test@example.com");
        _mockConfiguration.Setup(x => x["Email:Password"]).Returns("password");
        _mockConfiguration.Setup(x => x["Email:FromEmail"]).Returns("noreply@example.com");
        _mockConfiguration.Setup(x => x["Email:FromName"]).Returns("Virtual Queue System");

        _emailService = new EmailNotificationService(_mockConfiguration.Object, _mockLogger.Object);
    }
    #endregion

    #region SendEmailAsync Tests
    [Fact]
    public async Task SendEmailAsync_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var to = "test@example.com";
        var subject = "Test Subject";
        var body = "Test Body";

        // Act
        await _emailService.SendEmailAsync(to, subject, body);

        // Assert
        // Should not throw exception
        // In a real implementation, we would verify the email was sent
    }

    [Fact]
    public async Task SendEmailAsync_WithInvalidEmail_ShouldThrowException()
    {
        // Arrange
        var to = "invalid-email";
        var subject = "Test Subject";
        var body = "Test Body";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _emailService.SendEmailAsync(to, subject, body));
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptySubject_ShouldThrowException()
    {
        // Arrange
        var to = "test@example.com";
        var subject = "";
        var body = "Test Body";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _emailService.SendEmailAsync(to, subject, body));
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptyBody_ShouldThrowException()
    {
        // Arrange
        var to = "test@example.com";
        var subject = "Test Subject";
        var body = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _emailService.SendEmailAsync(to, subject, body));
    }

    [Fact]
    public async Task SendEmailAsync_WithNullEmail_ShouldThrowException()
    {
        // Arrange
        string? to = null;
        var subject = "Test Subject";
        var body = "Test Body";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _emailService.SendEmailAsync(to!, subject, body));
    }
    #endregion

    #region SendSmsAsync Tests
    [Fact]
    public async Task SendSmsAsync_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var message = "Test SMS Message";

        // Act
        await _emailService.SendSmsAsync(phoneNumber, message);

        // Assert
        // Should not throw exception
        // In a real implementation, we would verify the SMS was sent
    }

    [Fact]
    public async Task SendSmsAsync_WithEmptyMessage_ShouldThrowException()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var message = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _emailService.SendSmsAsync(phoneNumber, message));
    }

    [Fact]
    public async Task SendSmsAsync_WithNullMessage_ShouldThrowException()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        string? message = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _emailService.SendSmsAsync(phoneNumber, message!));
    }
    #endregion

    #region SendWhatsAppAsync Tests
    [Fact]
    public async Task SendWhatsAppAsync_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var message = "Test WhatsApp Message";

        // Act
        await _emailService.SendWhatsAppAsync(phoneNumber, message);

        // Assert
        // Should not throw exception
        // In a real implementation, we would verify the WhatsApp message was sent
    }

    [Fact]
    public async Task SendWhatsAppAsync_WithEmptyMessage_ShouldThrowException()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var message = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _emailService.SendWhatsAppAsync(phoneNumber, message));
    }

    [Fact]
    public async Task SendWhatsAppAsync_WithNullMessage_ShouldThrowException()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        string? message = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _emailService.SendWhatsAppAsync(phoneNumber, message!));
    }
    #endregion

    #region SendBulkEmailAsync Tests
    [Fact]
    public async Task SendBulkEmailAsync_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var recipients = new List<string> { "test1@example.com", "test2@example.com" };
        var subject = "Test Subject";
        var body = "Test Body";

        // Act
        await _emailService.SendBulkEmailAsync(recipients, subject, body);

        // Assert
        // Should not throw exception
        // In a real implementation, we would verify the emails were sent
    }

    [Fact]
    public async Task SendBulkEmailAsync_WithEmptyRecipients_ShouldThrowException()
    {
        // Arrange
        var recipients = new List<string>();
        var subject = "Test Subject";
        var body = "Test Body";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _emailService.SendBulkEmailAsync(recipients, subject, body));
    }

    [Fact]
    public async Task SendBulkEmailAsync_WithNullRecipients_ShouldThrowException()
    {
        // Arrange
        List<string>? recipients = null;
        var subject = "Test Subject";
        var body = "Test Body";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _emailService.SendBulkEmailAsync(recipients!, subject, body));
    }
    #endregion
}
