using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Domain.Exceptions;

namespace VirtualQueue.Infrastructure.Services;

/// <summary>
/// Service for sending email notifications.
/// </summary>
public class EmailNotificationService : INotificationService
{
    #region Fields
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly EmailSettings _settings;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailNotificationService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="settings">The email settings.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of the required parameters are null.
    /// </exception>
    public EmailNotificationService(ILogger<EmailNotificationService> logger, IOptions<EmailSettings> settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
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
    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        ValidateEmailParameters(to, subject, body);
        
        try
        {
            _logger.LogInformation("Sending email to {Email} with subject: {Subject}", to, subject);
            
            // In a real implementation, you would use an email service like SendGrid, AWS SES, etc.
            // For demo purposes, we'll just log the email
            _logger.LogInformation("Email sent to {Email}: {Subject}\n{Body}", to, subject, body);
            
            // Simulate email sending delay
            await Task.Delay(100, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Email sending cancelled for {Email}", to);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            throw new NotificationException($"Failed to send email to {to}", ex);
        }
    }

    public async Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending SMS to {PhoneNumber}: {Message}", phoneNumber, message);
            
            // In a real implementation, you would use an SMS service like Twilio, AWS SNS, etc.
            // For demo purposes, we'll just log the SMS
            _logger.LogInformation("SMS sent to {PhoneNumber}: {Message}", phoneNumber, message);
            
            // Simulate SMS sending delay
            await Task.Delay(50, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
            throw;
        }
    }

    public async Task SendWhatsAppAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending WhatsApp message to {PhoneNumber}: {Message}", phoneNumber, message);
            
            // In a real implementation, you would use WhatsApp Business API
            // For demo purposes, we'll just log the WhatsApp message
            _logger.LogInformation("WhatsApp message sent to {PhoneNumber}: {Message}", phoneNumber, message);
            
            // Simulate WhatsApp sending delay
            await Task.Delay(75, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp message to {PhoneNumber}", phoneNumber);
            throw;
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Validates email parameters.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="body">The email body.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when any parameter is null, empty, or invalid.
    /// </exception>
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

/// <summary>
/// Configuration settings for email notifications.
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// Gets or sets the SMTP server address.
    /// </summary>
    public string SmtpServer { get; set; } = "localhost";
    
    /// <summary>
    /// Gets or sets the SMTP server port.
    /// </summary>
    public int SmtpPort { get; set; } = 587;
    
    /// <summary>
    /// Gets or sets the SMTP username.
    /// </summary>
    public string Username { get; set; } = "";
    
    /// <summary>
    /// Gets or sets the SMTP password.
    /// </summary>
    public string Password { get; set; } = "";
    
    /// <summary>
    /// Gets or sets a value indicating whether SSL is enabled.
    /// </summary>
    public bool EnableSsl { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the from email address.
    /// </summary>
    public string FromEmail { get; set; } = "noreply@virtualqueue.com";
    
    /// <summary>
    /// Gets or sets the from name.
    /// </summary>
    public string FromName { get; set; } = "Virtual Queue System";
}
