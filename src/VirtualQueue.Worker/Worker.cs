using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VirtualQueue.Application.Commands.UserSessions;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Domain.Entities;

namespace VirtualQueue.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(
        ILogger<Worker> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Virtual Queue Worker started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessQueuesAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Run every minute
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing queues");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes on error
            }
        }

        _logger.LogInformation("Virtual Queue Worker stopped at: {time}", DateTimeOffset.Now);
    }

    private async Task ProcessQueuesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var queueRepository = scope.ServiceProvider.GetRequiredService<IQueueRepository>();
        
        var activeQueues = await queueRepository.GetActiveQueuesAsync(cancellationToken);
        
        foreach (var queue in activeQueues)
        {
            try
            {
                await ProcessQueueAsync(queue, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing queue {QueueId} for tenant {TenantId}", 
                    queue.Id, queue.TenantId);
            }
        }
    }

    private async Task ProcessQueueAsync(Queue queue, CancellationToken cancellationToken)
    {
        // Check if it's time to release users based on the release rate
        var timeSinceLastRelease = queue.LastReleaseAt.HasValue 
            ? DateTime.UtcNow - queue.LastReleaseAt.Value 
            : TimeSpan.FromMinutes(1);

        var releaseInterval = TimeSpan.FromMinutes(1.0 / queue.ReleaseRatePerMinute);
        
        if (timeSinceLastRelease < releaseInterval)
        {
            return; // Not time to release yet
        }

        // Get waiting users count
        using var scope = _serviceProvider.CreateScope();
        var userSessionRepository = scope.ServiceProvider.GetRequiredService<IUserSessionRepository>();
        var waitingCount = await userSessionRepository.GetWaitingUsersCountByQueueIdAsync(queue.Id, cancellationToken);
        
        if (waitingCount == 0)
        {
            return; // No users to release
        }

        // Calculate how many users to release
        var usersToRelease = Math.Min(
            queue.ReleaseRatePerMinute,
            waitingCount);

        // Release users
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var command = new ReleaseUsersCommand(queue.TenantId, queue.Id, usersToRelease);
        var releasedCount = await mediator.Send(command, cancellationToken);

        if (releasedCount > 0)
        {
            _logger.LogInformation("Released {ReleasedCount} users from queue {QueueId} for tenant {TenantId}", 
                releasedCount, queue.Id, queue.TenantId);

            // Update metrics
            QueueProcessingMetrics.UsersReleasedTotal.Inc(releasedCount);
            QueueProcessingMetrics.QueueProcessingTotal.WithLabels(queue.Id.ToString(), queue.TenantId.ToString()).Inc();
        }
    }
}
