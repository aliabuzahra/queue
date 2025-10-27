namespace VirtualQueue.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    Task SendWhatsAppAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}

public interface IQueueNotificationService
{
    Task NotifyUserEnqueuedAsync(Guid tenantId, Guid queueId, string userIdentifier, int position, CancellationToken cancellationToken = default);
    Task NotifyUserPositionUpdateAsync(Guid tenantId, Guid queueId, string userIdentifier, int position, CancellationToken cancellationToken = default);
    Task NotifyUserReleasedAsync(Guid tenantId, Guid queueId, string userIdentifier, CancellationToken cancellationToken = default);
    Task NotifyUserDroppedAsync(Guid tenantId, Guid queueId, string userIdentifier, CancellationToken cancellationToken = default);
    Task NotifyQueueOpeningAsync(Guid tenantId, Guid queueId, DateTime openingTime, CancellationToken cancellationToken = default);
    Task NotifyQueueClosingAsync(Guid tenantId, Guid queueId, DateTime closingTime, CancellationToken cancellationToken = default);
}
