namespace VirtualQueue.Application.DTOs;

public record DashboardDto(
    Guid TenantId,
    string TenantName,
    DateTime GeneratedAt,
    TenantOverview Overview,
    List<QueueSummary> Queues,
    SystemHealth Health,
    List<Alert> Alerts,
    List<RecentActivity> RecentActivity
);

public record TenantOverview(
    int TotalQueues,
    int ActiveQueues,
    int TotalUsersToday,
    int UsersServedToday,
    double AverageWaitTime,
    double SystemEfficiency,
    double UserSatisfaction
);

public record QueueSummary(
    Guid QueueId,
    string QueueName,
    bool IsActive,
    int CurrentUsers,
    int UsersServedToday,
    double AverageWaitTime,
    string Status,
    DateTime? LastActivity
);

public record SystemHealth(
    string Status, // "healthy", "warning", "critical"
    int ActiveConnections,
    double CpuUsage,
    double MemoryUsage,
    int DatabaseConnections,
    int RedisConnections,
    DateTime LastHealthCheck
);

public record Alert(
    string Id,
    string Type, // "performance", "capacity", "error", "maintenance"
    string Severity, // "low", "medium", "high", "critical"
    string Title,
    string Description,
    DateTime CreatedAt,
    bool IsResolved,
    DateTime? ResolvedAt
);

public record RecentActivity(
    string Id,
    string Type, // "user_enqueued", "user_released", "queue_created", "queue_updated"
    string Description,
    DateTime Timestamp,
    string UserIdentifier,
    Guid? QueueId,
    string QueueName
);
