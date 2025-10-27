using Microsoft.AspNetCore.SignalR;
using VirtualQueue.Api.Hubs;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Api.Services;

public class SignalRService : ISignalRService
{
    private readonly IHubContext<QueueHub> _hubContext;

    public SignalRService(IHubContext<QueueHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyQueueUpdateAsync(Guid tenantId, Guid queueId, object data)
    {
        var groupName = $"queue_{tenantId}_{queueId}";
        await _hubContext.Clients.Group(groupName).SendAsync("QueueUpdated", data);
    }

    public async Task NotifyUserUpdateAsync(Guid tenantId, string userIdentifier, object data)
    {
        var groupName = $"user_{tenantId}_{userIdentifier}";
        await _hubContext.Clients.Group(groupName).SendAsync("UserUpdated", data);
    }

    public async Task NotifyQueuePositionUpdateAsync(Guid tenantId, Guid queueId, string userIdentifier, int position)
    {
        var queueGroupName = $"queue_{tenantId}_{queueId}";
        var userGroupName = $"user_{tenantId}_{userIdentifier}";
        
        var positionData = new
        {
            QueueId = queueId,
            UserIdentifier = userIdentifier,
            Position = position,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group(queueGroupName).SendAsync("PositionUpdated", positionData);
        await _hubContext.Clients.Group(userGroupName).SendAsync("PositionUpdated", positionData);
    }

    public async Task NotifyUserReleasedAsync(Guid tenantId, Guid queueId, string userIdentifier)
    {
        var queueGroupName = $"queue_{tenantId}_{queueId}";
        var userGroupName = $"user_{tenantId}_{userIdentifier}";
        
        var releaseData = new
        {
            QueueId = queueId,
            UserIdentifier = userIdentifier,
            ReleasedAt = DateTime.UtcNow
        };

        await _hubContext.Clients.Group(queueGroupName).SendAsync("UserReleased", releaseData);
        await _hubContext.Clients.Group(userGroupName).SendAsync("UserReleased", releaseData);
    }

    public async Task NotifyQueueStatisticsAsync(Guid tenantId, Guid queueId, object statistics)
    {
        var groupName = $"queue_{tenantId}_{queueId}";
        await _hubContext.Clients.Group(groupName).SendAsync("QueueStatistics", statistics);
    }
}
