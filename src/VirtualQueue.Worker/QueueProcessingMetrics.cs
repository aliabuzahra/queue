using Prometheus;

namespace VirtualQueue.Worker;

public static class QueueProcessingMetrics
{
    // User metrics
    public static readonly Counter UsersReleasedTotal = Metrics
        .CreateCounter("virtualqueue_users_released_total", "Total number of users released from queues");

    public static readonly Counter UsersEnqueuedTotal = Metrics
        .CreateCounter("virtualqueue_users_enqueued_total", "Total number of users enqueued", 
            new[] { "queue_id", "tenant_id" });

    public static readonly Counter UsersDroppedTotal = Metrics
        .CreateCounter("virtualqueue_users_dropped_total", "Total number of users dropped from queues", 
            new[] { "queue_id", "tenant_id" });

    // Queue metrics
    public static readonly Counter QueueProcessingTotal = Metrics
        .CreateCounter("virtualqueue_queue_processing_total", "Total number of queue processing operations", 
            new[] { "queue_id", "tenant_id" });

    public static readonly Gauge ActiveQueues = Metrics
        .CreateGauge("virtualqueue_active_queues", "Number of active queues");

    public static readonly Gauge WaitingUsers = Metrics
        .CreateGauge("virtualqueue_waiting_users", "Number of users waiting in queues", 
            new[] { "queue_id", "tenant_id" });

    public static readonly Gauge ServingUsers = Metrics
        .CreateGauge("virtualqueue_serving_users", "Number of users currently being served", 
            new[] { "queue_id", "tenant_id" });

    public static readonly Gauge ReleasedUsers = Metrics
        .CreateGauge("virtualqueue_released_users", "Number of users released from queues", 
            new[] { "queue_id", "tenant_id" });

    // Performance metrics
    public static readonly Histogram QueueProcessingDuration = Metrics
        .CreateHistogram("virtualqueue_queue_processing_duration_seconds", "Time spent processing queues", 
            new[] { "queue_id", "tenant_id" });

    public static readonly Histogram UserWaitTime = Metrics
        .CreateHistogram("virtualqueue_user_wait_time_seconds", "Time users spend waiting in queue", 
            new[] { "queue_id", "tenant_id" });

    // System metrics
    public static readonly Gauge TotalTenants = Metrics
        .CreateGauge("virtualqueue_total_tenants", "Total number of tenants");

    public static readonly Gauge TotalQueues = Metrics
        .CreateGauge("virtualqueue_total_queues", "Total number of queues");

    public static readonly Gauge TotalUserSessions = Metrics
        .CreateGauge("virtualqueue_total_user_sessions", "Total number of user sessions");
}
