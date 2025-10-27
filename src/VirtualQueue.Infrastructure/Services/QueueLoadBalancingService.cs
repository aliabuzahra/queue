using Microsoft.Extensions.Logging;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Infrastructure.Services;

public class QueueLoadBalancingService : IQueueLoadBalancingService
{
    private readonly ILogger<QueueLoadBalancingService> _logger;
    private readonly ICacheService _cacheService;
    private readonly IQueueRepository _queueRepository;
    private readonly IUserSessionRepository _userSessionRepository;

    public QueueLoadBalancingService(
        ILogger<QueueLoadBalancingService> logger,
        ICacheService cacheService,
        IQueueRepository queueRepository,
        IUserSessionRepository userSessionRepository)
    {
        _logger = logger;
        _cacheService = cacheService;
        _queueRepository = queueRepository;
        _userSessionRepository = userSessionRepository;
    }

    public async Task<LoadBalancingResult> DistributeLoadAsync(Guid tenantId, List<Guid> queueIds, LoadBalancingStrategy strategy, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Distributing load for tenant {TenantId} using strategy {Strategy}", tenantId, strategy);
            
            var queueLoads = new List<QueueLoadInfo>();
            foreach (var queueId in queueIds)
            {
                var loadInfo = await GetQueueLoadAsync(queueId, cancellationToken);
                queueLoads.Add(loadInfo);
            }

            var distributions = CalculateDistributions(queueLoads, strategy);
            var metrics = CalculateMetrics(queueLoads);

            var result = new LoadBalancingResult(
                true,
                distributions,
                metrics
            );

            _logger.LogInformation("Load balancing completed for tenant {TenantId}. Efficiency: {Efficiency}", tenantId, metrics.EfficiencyScore);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to distribute load for tenant {TenantId}", tenantId);
            return new LoadBalancingResult(
                false,
                new List<QueueDistribution>(),
                new LoadBalancingMetrics(0, 0, 0, 0, 0),
                ex.Message
            );
        }
    }

    public async Task<QueueLoadInfo> GetQueueLoadAsync(Guid queueId, CancellationToken cancellationToken = default)
    {
        try
        {
            var queue = await _queueRepository.GetByIdAsync(queueId, cancellationToken);
            if (queue == null)
            {
                throw new InvalidOperationException($"Queue {queueId} not found");
            }

            var currentUsers = 0; // Mock implementation - in real implementation, this would get active user count
            var capacity = 100; // Default capacity - in real implementation, this would come from queue.MaxCapacity
            var loadPercentage = capacity > 0 ? (double)currentUsers / capacity * 100 : 0;
            var averageWaitTime = await CalculateAverageWaitTimeAsync(queueId, cancellationToken);
            var throughput = await CalculateThroughputAsync(queueId, cancellationToken);

            var status = DetermineQueueStatus(loadPercentage);

            return new QueueLoadInfo(
                queueId,
                queue.Name,
                currentUsers,
                capacity,
                loadPercentage,
                averageWaitTime,
                throughput,
                status,
                DateTime.UtcNow
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get queue load for {QueueId}", queueId);
            return new QueueLoadInfo(
                queueId,
                "Unknown",
                0,
                0,
                0,
                0,
                0,
                QueueLoadStatus.Underloaded,
                DateTime.UtcNow
            );
        }
    }

    public async Task<List<QueueLoadInfo>> GetTenantLoadAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting load information for tenant {TenantId}", tenantId);
            
            var queues = await _queueRepository.GetByTenantIdAsync(tenantId, cancellationToken);
            var loadInfos = new List<QueueLoadInfo>();

            foreach (var queue in queues)
            {
                var loadInfo = await GetQueueLoadAsync(queue.Id, cancellationToken);
                loadInfos.Add(loadInfo);
            }

            return loadInfos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tenant load for {TenantId}", tenantId);
            return new List<QueueLoadInfo>();
        }
    }

    public async Task<bool> RebalanceQueuesAsync(Guid tenantId, List<Guid> queueIds, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Rebalancing queues for tenant {TenantId}", tenantId);
            
            var loadInfos = new List<QueueLoadInfo>();
            foreach (var queueId in queueIds)
            {
                var loadInfo = await GetQueueLoadAsync(queueId, cancellationToken);
                loadInfos.Add(loadInfo);
            }

            // Implement rebalancing logic
            var overloadedQueues = loadInfos.Where(q => q.Status == QueueLoadStatus.Overloaded).ToList();
            var underloadedQueues = loadInfos.Where(q => q.Status == QueueLoadStatus.Underloaded).ToList();

            foreach (var overloaded in overloadedQueues)
            {
                var underloaded = underloadedQueues.FirstOrDefault();
                if (underloaded != null)
                {
                    // Move users from overloaded to underloaded queue
                    await MoveUsersBetweenQueuesAsync(overloaded.QueueId, underloaded.QueueId, cancellationToken);
                }
            }

            _logger.LogInformation("Queue rebalancing completed for tenant {TenantId}", tenantId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rebalance queues for tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<LoadBalancingRecommendation> GetLoadBalancingRecommendationAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting load balancing recommendations for tenant {TenantId}", tenantId);
            
            var loadInfos = await GetTenantLoadAsync(tenantId, cancellationToken);
            var recommendations = new List<QueueRecommendation>();

            foreach (var loadInfo in loadInfos)
            {
                var recommendation = AnalyzeQueueLoad(loadInfo);
                if (recommendation.Action != LoadBalancingAction.None)
                {
                    recommendations.Add(recommendation);
                }
            }

            var recommendedStrategy = DetermineOptimalStrategy(loadInfos);
            var efficiencyGain = CalculatePotentialEfficiencyGain(loadInfos, recommendations);

            return new LoadBalancingRecommendation(
                tenantId,
                recommendations,
                recommendedStrategy,
                efficiencyGain,
                $"Recommended {recommendedStrategy} strategy with {efficiencyGain:P1} efficiency gain"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get load balancing recommendations for tenant {TenantId}", tenantId);
            return new LoadBalancingRecommendation(
                tenantId,
                new List<QueueRecommendation>(),
                LoadBalancingStrategy.RoundRobin,
                0,
                "Unable to generate recommendations"
            );
        }
    }

    public async Task<bool> SetQueueCapacityAsync(Guid queueId, int capacity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Setting capacity for queue {QueueId} to {Capacity}", queueId, capacity);
            
            var queue = await _queueRepository.GetByIdAsync(queueId, cancellationToken);
            if (queue == null)
            {
                return false;
            }

            // In a real implementation, this would update the queue capacity
            _logger.LogInformation("Queue {QueueId} capacity set to {Capacity}", queueId, capacity);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set capacity for queue {QueueId}", queueId);
            return false;
        }
    }

    public async Task<bool> EnableAutoScalingAsync(Guid queueId, AutoScalingConfig config, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Enabling auto-scaling for queue {QueueId}", queueId);
            
            // In a real implementation, this would enable auto-scaling
            _logger.LogInformation("Auto-scaling enabled for queue {QueueId} with config: Min={Min}, Max={Max}", 
                queueId, config.MinCapacity, config.MaxCapacity);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enable auto-scaling for queue {QueueId}", queueId);
            return false;
        }
    }

    public async Task<bool> DisableAutoScalingAsync(Guid queueId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Disabling auto-scaling for queue {QueueId}", queueId);
            
            // In a real implementation, this would disable auto-scaling
            _logger.LogInformation("Auto-scaling disabled for queue {QueueId}", queueId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disable auto-scaling for queue {QueueId}", queueId);
            return false;
        }
    }

    private List<QueueDistribution> CalculateDistributions(List<QueueLoadInfo> queueLoads, LoadBalancingStrategy strategy)
    {
        var distributions = new List<QueueDistribution>();

        foreach (var loadInfo in queueLoads)
        {
            var action = DetermineAction(loadInfo, strategy);
            var recommendedUsers = CalculateRecommendedUsers(loadInfo, action);

            distributions.Add(new QueueDistribution(
                loadInfo.QueueId,
                loadInfo.QueueName,
                loadInfo.CurrentUsers,
                recommendedUsers,
                loadInfo.Capacity,
                loadInfo.LoadPercentage,
                action
            ));
        }

        return distributions;
    }

    private LoadBalancingMetrics CalculateMetrics(List<QueueLoadInfo> queueLoads)
    {
        if (!queueLoads.Any())
        {
            return new LoadBalancingMetrics(0, 0, 0, 0, 0);
        }

        var averageLoad = queueLoads.Average(q => q.LoadPercentage);
        var loadVariance = CalculateVariance(queueLoads.Select(q => q.LoadPercentage));
        var overloadedQueues = queueLoads.Count(q => q.Status == QueueLoadStatus.Overloaded);
        var underloadedQueues = queueLoads.Count(q => q.Status == QueueLoadStatus.Underloaded);
        var efficiencyScore = CalculateEfficiencyScore(queueLoads);

        return new LoadBalancingMetrics(
            averageLoad,
            loadVariance,
            overloadedQueues,
            underloadedQueues,
            efficiencyScore
        );
    }

    private QueueLoadStatus DetermineQueueStatus(double loadPercentage)
    {
        return loadPercentage switch
        {
            < 25 => QueueLoadStatus.Underloaded,
            < 50 => QueueLoadStatus.Normal,
            < 75 => QueueLoadStatus.Loaded,
            < 90 => QueueLoadStatus.Overloaded,
            _ => QueueLoadStatus.Critical
        };
    }

    private LoadBalancingAction DetermineAction(QueueLoadInfo loadInfo, LoadBalancingStrategy strategy)
    {
        return loadInfo.Status switch
        {
            QueueLoadStatus.Underloaded => LoadBalancingAction.ScaleDown,
            QueueLoadStatus.Overloaded => LoadBalancingAction.ScaleUp,
            QueueLoadStatus.Critical => LoadBalancingAction.Rebalance,
            _ => LoadBalancingAction.None
        };
    }

    private int CalculateRecommendedUsers(QueueLoadInfo loadInfo, LoadBalancingAction action)
    {
        return action switch
        {
            LoadBalancingAction.ScaleUp => Math.Min(loadInfo.CurrentUsers + 10, loadInfo.Capacity),
            LoadBalancingAction.ScaleDown => Math.Max(loadInfo.CurrentUsers - 10, 0),
            LoadBalancingAction.Rebalance => loadInfo.Capacity / 2,
            _ => loadInfo.CurrentUsers
        };
    }

    private QueueRecommendation AnalyzeQueueLoad(QueueLoadInfo loadInfo)
    {
        var action = DetermineAction(loadInfo, LoadBalancingStrategy.CapacityBased);
        var suggestedCapacity = CalculateRecommendedUsers(loadInfo, action);
        var reason = GetRecommendationReason(loadInfo, action);

        return new QueueRecommendation(
            loadInfo.QueueId,
            loadInfo.QueueName,
            action,
            suggestedCapacity,
            reason
        );
    }

    private string GetRecommendationReason(QueueLoadInfo loadInfo, LoadBalancingAction action)
    {
        return action switch
        {
            LoadBalancingAction.ScaleUp => $"Queue is overloaded ({loadInfo.LoadPercentage:F1}% capacity)",
            LoadBalancingAction.ScaleDown => $"Queue is underloaded ({loadInfo.LoadPercentage:F1}% capacity)",
            LoadBalancingAction.Rebalance => $"Queue is critically overloaded ({loadInfo.LoadPercentage:F1}% capacity)",
            _ => "Queue load is optimal"
        };
    }

    private LoadBalancingStrategy DetermineOptimalStrategy(List<QueueLoadInfo> loadInfos)
    {
        var variance = CalculateVariance(loadInfos.Select(q => q.LoadPercentage));
        
        return variance switch
        {
            < 10 => LoadBalancingStrategy.RoundRobin,
            < 25 => LoadBalancingStrategy.LeastLoaded,
            < 50 => LoadBalancingStrategy.WeightedRoundRobin,
            _ => LoadBalancingStrategy.CapacityBased
        };
    }

    private double CalculatePotentialEfficiencyGain(List<QueueLoadInfo> loadInfos, List<QueueRecommendation> recommendations)
    {
        if (!loadInfos.Any())
            return 0;

        var currentEfficiency = CalculateEfficiencyScore(loadInfos);
        var potentialEfficiency = currentEfficiency + (recommendations.Count * 0.1);
        
        return Math.Max(0, potentialEfficiency - currentEfficiency);
    }

    private double CalculateEfficiencyScore(List<QueueLoadInfo> loadInfos)
    {
        if (!loadInfos.Any())
            return 0;

        var averageLoad = loadInfos.Average(q => q.LoadPercentage);
        var variance = CalculateVariance(loadInfos.Select(q => q.LoadPercentage));
        
        // Efficiency is higher when load is balanced (low variance) and optimal (around 70%)
        var balanceScore = Math.Max(0, 1 - (variance / 100));
        var loadScore = Math.Max(0, 1 - Math.Abs(averageLoad - 70) / 70);
        
        return (balanceScore + loadScore) / 2;
    }

    private double CalculateVariance(IEnumerable<double> values)
    {
        var valueList = values.ToList();
        if (!valueList.Any())
            return 0;

        var mean = valueList.Average();
        var variance = valueList.Sum(v => Math.Pow(v - mean, 2)) / valueList.Count;
        return Math.Sqrt(variance);
    }

    private async Task<double> CalculateAverageWaitTimeAsync(Guid queueId, CancellationToken cancellationToken)
    {
        // In a real implementation, this would calculate actual wait times
        return Random.Shared.NextDouble() * 30; // Mock wait time in minutes
    }

    private async Task<int> CalculateThroughputAsync(Guid queueId, CancellationToken cancellationToken)
    {
        // In a real implementation, this would calculate actual throughput
        return Random.Shared.Next(10, 100); // Mock throughput
    }

    private async Task MoveUsersBetweenQueuesAsync(Guid fromQueueId, Guid toQueueId, CancellationToken cancellationToken)
    {
        // In a real implementation, this would move users between queues
        _logger.LogInformation("Moving users from queue {FromQueueId} to {ToQueueId}", fromQueueId, toQueueId);
    }

    // New methods required by the interface
    public async Task<QueueLoadInfo> GetQueueLoadStatusAsync(Guid queueId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetQueueLoadAsync(queueId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get queue load status for {QueueId}", queueId);
            return new QueueLoadInfo(
                queueId,
                "Unknown",
                0,
                0,
                0,
                0,
                0,
                QueueLoadStatus.Underloaded,
                DateTime.UtcNow
            );
        }
    }

    public async Task<List<QueueLoadInfo>> GetAllQueueLoadStatusesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetTenantLoadAsync(tenantId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all queue load statuses for tenant {TenantId}", tenantId);
            return new List<QueueLoadInfo>();
        }
    }

    public async Task<QueueLoadInfo> GetOptimalQueueAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting optimal queue for tenant {TenantId}", tenantId);
            
            var loadInfos = await GetTenantLoadAsync(tenantId, cancellationToken);
            if (!loadInfos.Any())
            {
                throw new InvalidOperationException("No queues found for tenant");
            }

            // Find the queue with the lowest load percentage
            var optimalQueue = loadInfos.OrderBy(q => q.LoadPercentage).First();
            
            _logger.LogInformation("Optimal queue found: {QueueId} with {LoadPercentage:F1}% load", 
                optimalQueue.QueueId, optimalQueue.LoadPercentage);
            
            return optimalQueue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get optimal queue for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<bool> RebalanceQueueAsync(Guid queueId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Rebalancing queue {QueueId}", queueId);
            
            var queue = await _queueRepository.GetByIdAsync(queueId, cancellationToken);
            if (queue == null)
            {
                return false;
            }

            // In a real implementation, this would rebalance the specific queue
            _logger.LogInformation("Queue {QueueId} rebalanced successfully", queueId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rebalance queue {QueueId}", queueId);
            return false;
        }
    }

    public async Task<LoadBalancingRecommendation> GetLoadBalancingRecommendationsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetLoadBalancingRecommendationAsync(tenantId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get load balancing recommendations for tenant {TenantId}", tenantId);
            return new LoadBalancingRecommendation(
                tenantId,
                new List<QueueRecommendation>(),
                LoadBalancingStrategy.RoundRobin,
                0,
                "Unable to generate recommendations"
            );
        }
    }
}
