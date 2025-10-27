using Microsoft.AspNetCore.SignalR;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Api.Hubs;

public class QueueHub : Hub
{
    private readonly ITenantContext _tenantContext;

    public QueueHub(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public async Task JoinQueueGroup(Guid queueId)
    {
        if (!_tenantContext.IsValid)
        {
            await Clients.Caller.SendAsync("Error", "Invalid tenant context");
            return;
        }

        var groupName = $"queue_{_tenantContext.TenantId}_{queueId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.SendAsync("JoinedQueue", queueId);
    }

    public async Task LeaveQueueGroup(Guid queueId)
    {
        if (!_tenantContext.IsValid)
        {
            await Clients.Caller.SendAsync("Error", "Invalid tenant context");
            return;
        }

        var groupName = $"queue_{_tenantContext.TenantId}_{queueId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.SendAsync("LeftQueue", queueId);
    }

    public async Task SubscribeToUserUpdates(string userIdentifier)
    {
        if (!_tenantContext.IsValid)
        {
            await Clients.Caller.SendAsync("Error", "Invalid tenant context");
            return;
        }

        var groupName = $"user_{_tenantContext.TenantId}_{userIdentifier}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.SendAsync("SubscribedToUser", userIdentifier);
    }

    public async Task UnsubscribeFromUserUpdates(string userIdentifier)
    {
        if (!_tenantContext.IsValid)
        {
            await Clients.Caller.SendAsync("Error", "Invalid tenant context");
            return;
        }

        var groupName = $"user_{_tenantContext.TenantId}_{userIdentifier}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.SendAsync("UnsubscribedFromUser", userIdentifier);
    }
}
