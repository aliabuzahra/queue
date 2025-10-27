namespace VirtualQueue.Application.Common.Interfaces;

public interface ISignalRService
{
    Task NotifyQueueUpdateAsync(Guid tenantId, Guid queueId, object data);
    Task NotifyUserUpdateAsync(Guid tenantId, string userIdentifier, object data);
    Task NotifyQueuePositionUpdateAsync(Guid tenantId, Guid queueId, string userIdentifier, int position);
    Task NotifyUserReleasedAsync(Guid tenantId, Guid queueId, string userIdentifier);
    Task NotifyQueueStatisticsAsync(Guid tenantId, Guid queueId, object statistics);
}
