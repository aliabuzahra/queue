namespace VirtualQueue.Application.DTOs;

public record NotificationTemplate(
    string Subject,
    string Body,
    NotificationType Type
);

public enum NotificationType
{
    Email,
    Sms,
    WhatsApp
}

public record NotificationSettings(
    bool EmailEnabled,
    bool SmsEnabled,
    bool WhatsAppEnabled,
    string? EmailTemplate,
    string? SmsTemplate,
    string? WhatsAppTemplate
);
