namespace VirtualQueue.Application.DTOs;

public record QueueAnalyticsDto(
    Guid QueueId,
    string QueueName,
    int TotalUsers,
    int WaitingUsers,
    int ServingUsers,
    int ReleasedUsers,
    int DroppedUsers,
    double AverageWaitTimeMinutes,
    double AverageServingTimeMinutes,
    DateTime LastReleaseAt,
    int TotalReleasesToday
);
