namespace VirtualQueue.Application.Common.Interfaces;

public interface IQueueLoadBalancingService
{
    // Methods expected by controllers
    Task<QueueLoadInfo> GetQueueLoadStatusAsync(Guid queueId, CancellationToken cancellationToken = default);
    Task<List<QueueLoadInfo>> GetAllQueueLoadStatusesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<QueueLoadInfo> GetOptimalQueueAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> RebalanceQueueAsync(Guid queueId, CancellationToken cancellationToken = default);
    Task<LoadBalancingRecommendation> GetLoadBalancingRecommendationsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> EnableAutoScalingAsync(Guid queueId, AutoScalingConfig config, CancellationToken cancellationToken = default);
    Task<bool> DisableAutoScalingAsync(Guid queueId, CancellationToken cancellationToken = default);
    
    // Original methods
    Task<LoadBalancingResult> DistributeLoadAsync(Guid tenantId, List<Guid> queueIds, LoadBalancingStrategy strategy, CancellationToken cancellationToken = default);
    Task<QueueLoadInfo> GetQueueLoadAsync(Guid queueId, CancellationToken cancellationToken = default);
    Task<List<QueueLoadInfo>> GetTenantLoadAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> RebalanceQueuesAsync(Guid tenantId, List<Guid> queueIds, CancellationToken cancellationToken = default);
    Task<LoadBalancingRecommendation> GetLoadBalancingRecommendationAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> SetQueueCapacityAsync(Guid queueId, int capacity, CancellationToken cancellationToken = default);
}

public record LoadBalancingResult(
    bool Success,
    List<QueueDistribution> Distributions,
    LoadBalancingMetrics Metrics,
    string? ErrorMessage = null
);

public record QueueDistribution(
    Guid QueueId,
    string QueueName,
    int CurrentUsers,
    int RecommendedUsers,
    int Capacity,
    double LoadPercentage,
    LoadBalancingAction Action
);

public record LoadBalancingMetrics(
    double AverageLoad,
    double LoadVariance,
    int OverloadedQueues,
    int UnderloadedQueues,
    double EfficiencyScore
);

public record QueueLoadInfo(
    Guid QueueId,
    string QueueName,
    int CurrentUsers,
    int Capacity,
    double LoadPercentage,
    double AverageWaitTime,
    int Throughput,
    QueueLoadStatus Status,
    DateTime LastUpdated
);

public record LoadBalancingRecommendation(
    Guid TenantId,
    List<QueueRecommendation> Recommendations,
    LoadBalancingStrategy RecommendedStrategy,
    double PotentialEfficiencyGain,
    string Description
);

public record QueueRecommendation(
    Guid QueueId,
    string QueueName,
    LoadBalancingAction Action,
    int SuggestedCapacity,
    string Reason
);

public record AutoScalingConfig(
    int MinCapacity,
    int MaxCapacity,
    double ScaleUpThreshold,
    double ScaleDownThreshold,
    TimeSpan ScaleUpCooldown,
    TimeSpan ScaleDownCooldown
);

public enum LoadBalancingStrategy
{
    RoundRobin,
    LeastLoaded,
    WeightedRoundRobin,
    CapacityBased,
    PerformanceBased
}

public enum LoadBalancingAction
{
    None,
    ScaleUp,
    ScaleDown,
    Rebalance,
    Merge,
    Split
}

public enum QueueLoadStatus
{
    Underloaded,
    Normal,
    Loaded,
    Overloaded,
    Critical
}
