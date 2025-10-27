using Microsoft.Extensions.Logging;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Infrastructure.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IQueueRepository _queueRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IQueueRepository queueRepository,
        IUserSessionRepository userSessionRepository,
        ITenantRepository tenantRepository,
        ILogger<AnalyticsService> logger)
    {
        _queueRepository = queueRepository;
        _userSessionRepository = userSessionRepository;
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    public async Task<AdvancedAnalyticsDto> GetQueueAnalyticsAsync(Guid tenantId, Guid queueId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var queue = await _queueRepository.GetByTenantIdAndIdAsync(tenantId, queueId, cancellationToken);
        if (queue == null)
            throw new InvalidOperationException($"Queue with ID '{queueId}' not found for tenant");

        var timeRange = new TimeRange(
            startDate ?? DateTime.UtcNow.AddDays(-7),
            endDate ?? DateTime.UtcNow,
            "UTC"
        );

        // Get user sessions for the time range
        var userSessions = await _userSessionRepository.GetByQueueIdAndDateRangeAsync(queueId, timeRange.StartDate, timeRange.EndDate, cancellationToken);

        // Calculate metrics
        var metrics = CalculateQueueMetrics(userSessions, queue);
        var userMetrics = CalculateUserMetrics(userSessions);
        var performance = CalculatePerformanceMetrics(userSessions, timeRange);
        var trends = CalculateTrendAnalysis(userSessions, timeRange);
        var hourlyData = CalculateHourlyData(userSessions, timeRange);
        var dailyData = CalculateDailyData(userSessions, timeRange);

        return new AdvancedAnalyticsDto(
            queueId,
            queue.Name,
            DateTime.UtcNow,
            timeRange,
            metrics,
            userMetrics,
            performance,
            trends,
            hourlyData,
            dailyData
        );
    }

    public async Task<DashboardDto> GetTenantDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null)
            throw new InvalidOperationException($"Tenant with ID '{tenantId}' not found");

        var queues = await _queueRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        // Calculate overview metrics
        var totalUsersToday = await _userSessionRepository.GetUserCountByDateRangeAsync(tenantId, today, tomorrow, cancellationToken);
        var usersServedToday = await _userSessionRepository.GetReleasedUserCountByDateRangeAsync(tenantId, today, tomorrow, cancellationToken);

        var overview = new TenantOverview(
            queues.Count(),
            queues.Count(q => q.IsActive),
            totalUsersToday,
            usersServedToday,
            CalculateAverageWaitTime(queues.ToList()),
            CalculateSystemEfficiency(queues.ToList()),
            4.2 // Mock satisfaction score
        );

        var queueSummaries = queues.Select(q => new QueueSummary(
            q.Id,
            q.Name,
            q.IsActive,
            q.GetWaitingUsersCount(),
            GetUsersServedToday(q.Id, today),
            GetAverageWaitTime(q.Id),
            q.IsActive ? "Active" : "Inactive",
            q.LastReleaseAt
        )).ToList();

        var health = await GetSystemHealthAsync(cancellationToken);
        var alerts = await GetAlertsAsync(tenantId, cancellationToken);
        var recentActivity = await GetRecentActivityAsync(tenantId, cancellationToken);

        return new DashboardDto(
            tenantId,
            tenant.Name,
            DateTime.UtcNow,
            overview,
            queueSummaries,
            health,
            alerts,
            recentActivity
        );
    }

    public async Task<List<HourlyData>> GetHourlyAnalyticsAsync(Guid tenantId, Guid queueId, DateTime date, CancellationToken cancellationToken = default)
    {
        var startOfDay = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var endOfDay = startOfDay.AddDays(1);
        
        var userSessions = await _userSessionRepository.GetByQueueIdAndDateRangeAsync(queueId, startOfDay, endOfDay, cancellationToken);
        
        return CalculateHourlyData(userSessions, new TimeRange(startOfDay, endOfDay, "UTC"));
    }

    public async Task<List<DailyData>> GetDailyAnalyticsAsync(Guid tenantId, Guid queueId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var utcStartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        var utcEndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
        
        var userSessions = await _userSessionRepository.GetByQueueIdAndDateRangeAsync(queueId, utcStartDate, utcEndDate, cancellationToken);
        
        return CalculateDailyData(userSessions, new TimeRange(utcStartDate, utcEndDate, "UTC"));
    }

    public async Task<PerformanceMetrics> GetPerformanceMetricsAsync(Guid tenantId, Guid queueId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var timeRange = new TimeRange(
            startDate ?? DateTime.UtcNow.AddDays(-7),
            endDate ?? DateTime.UtcNow,
            "UTC"
        );

        var userSessions = await _userSessionRepository.GetByQueueIdAndDateRangeAsync(queueId, timeRange.StartDate, timeRange.EndDate, cancellationToken);
        
        return CalculatePerformanceMetrics(userSessions, timeRange);
    }

    public async Task<TrendAnalysis> GetTrendAnalysisAsync(Guid tenantId, Guid queueId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var timeRange = new TimeRange(
            startDate ?? DateTime.UtcNow.AddDays(-7),
            endDate ?? DateTime.UtcNow,
            "UTC"
        );

        var userSessions = await _userSessionRepository.GetByQueueIdAndDateRangeAsync(queueId, timeRange.StartDate, timeRange.EndDate, cancellationToken);
        
        return CalculateTrendAnalysis(userSessions, timeRange);
    }

    public async Task<List<Alert>> GetAlertsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        // Mock alerts - in a real implementation, these would come from a monitoring system
        return new List<Alert>
        {
            new Alert(
                Guid.NewGuid().ToString(),
                "performance",
                "medium",
                "High Queue Length",
                "Queue 'Main Queue' has 50+ users waiting",
                DateTime.UtcNow.AddHours(-2),
                false,
                null
            ),
            new Alert(
                Guid.NewGuid().ToString(),
                "capacity",
                "low",
                "Resource Usage",
                "System resources are at 75% capacity",
                DateTime.UtcNow.AddHours(-1),
                false,
                null
            )
        };
    }

    public async Task<SystemHealth> GetSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        // Mock system health - in a real implementation, this would check actual system metrics
        return new SystemHealth(
            "healthy",
            150,
            45.2,
            68.7,
            25,
            8,
            DateTime.UtcNow
        );
    }

    private QueueMetrics CalculateQueueMetrics(List<Domain.Entities.UserSession> userSessions, Domain.Entities.Queue queue)
    {
        var totalUsers = userSessions.Count;
        var waitingUsers = userSessions.Count(u => u.Status == Domain.Enums.QueueStatus.Waiting);
        var servingUsers = userSessions.Count(u => u.Status == Domain.Enums.QueueStatus.Serving);
        var releasedUsers = userSessions.Count(u => u.Status == Domain.Enums.QueueStatus.Released);
        var droppedUsers = userSessions.Count(u => u.Status == Domain.Enums.QueueStatus.Dropped);

        var averageWaitTime = userSessions
            .Where(u => u.ReleasedAt.HasValue)
            .Select(u => (u.ReleasedAt!.Value - u.EnqueuedAt).TotalMinutes)
            .DefaultIfEmpty(0)
            .Average();

        var averageServingTime = userSessions
            .Where(u => u.ServedAt.HasValue && u.ReleasedAt.HasValue)
            .Select(u => (u.ReleasedAt!.Value - u.ServedAt!.Value).TotalMinutes)
            .DefaultIfEmpty(0)
            .Average();

        return new QueueMetrics(
            totalUsers,
            waitingUsers,
            servingUsers,
            releasedUsers,
            droppedUsers,
            waitingUsers, // Average queue length
            waitingUsers, // Peak queue length
            averageWaitTime,
            averageServingTime,
            releasedUsers,
            releasedUsers, // Releases today
            queue.LastReleaseAt
        );
    }

    private UserMetrics CalculateUserMetrics(List<Domain.Entities.UserSession> userSessions)
    {
        var today = DateTime.UtcNow.Date;
        var newUsersToday = userSessions.Count(u => u.EnqueuedAt.Date == today);
        var vipUsers = userSessions.Count(u => u.Priority == Domain.Enums.QueuePriority.VIP);
        var premiumUsers = userSessions.Count(u => u.Priority == Domain.Enums.QueuePriority.Premium);
        var noShowCount = userSessions.Count(u => u.Status == Domain.Enums.QueueStatus.Dropped);

        return new UserMetrics(
            newUsersToday,
            0, // Returning users - would need historical data
            vipUsers,
            premiumUsers,
            4.2, // Mock satisfaction score
            noShowCount,
            noShowCount / (double)userSessions.Count * 100
        );
    }

    private PerformanceMetrics CalculatePerformanceMetrics(List<Domain.Entities.UserSession> userSessions, TimeRange timeRange)
    {
        var hours = (timeRange.EndDate - timeRange.StartDate).TotalHours;
        var throughput = userSessions.Count(u => u.Status == Domain.Enums.QueueStatus.Released) / hours;
        
        return new PerformanceMetrics(
            throughput,
            throughput * 1.5, // Peak throughput
            2.5, // Average processing time
            85.0, // Queue efficiency
            75.0, // Resource utilization
            0 // Bottlenecks detected
        );
    }

    private TrendAnalysis CalculateTrendAnalysis(List<Domain.Entities.UserSession> userSessions, TimeRange timeRange)
    {
        return new TrendAnalysis(
            new QueueTrend("stable", 0, "Queue length is stable"),
            new WaitTimeTrend("decreasing", -5.2, -1.5, "Wait times are improving"),
            new ThroughputTrend("increasing", 8.3, 12, "Throughput is increasing"),
            new UserSatisfactionTrend("stable", 0, 0, "Satisfaction remains stable")
        );
    }

    private List<HourlyData> CalculateHourlyData(List<Domain.Entities.UserSession> userSessions, TimeRange timeRange)
    {
        var hourlyData = new List<HourlyData>();
        
        for (int hour = 0; hour < 24; hour++)
        {
            var hourStart = timeRange.StartDate.AddHours(hour);
            var hourEnd = hourStart.AddHours(1);
            
            var hourSessions = userSessions.Where(u => u.EnqueuedAt >= hourStart && u.EnqueuedAt < hourEnd).ToList();
            
            hourlyData.Add(new HourlyData(
                hour,
                hourSessions.Count,
                hourSessions.Count(u => u.Status == Domain.Enums.QueueStatus.Released),
                hourSessions.Any() ? (int)hourSessions.Where(u => u.ReleasedAt.HasValue).Average(u => (u.ReleasedAt!.Value - u.EnqueuedAt).TotalMinutes) : 0,
                hourSessions.Count(u => u.Status == Domain.Enums.QueueStatus.Waiting),
                hourSessions.Count(u => u.Status == Domain.Enums.QueueStatus.Released)
            ));
        }
        
        return hourlyData;
    }

    private List<DailyData> CalculateDailyData(List<Domain.Entities.UserSession> userSessions, TimeRange timeRange)
    {
        var dailyData = new List<DailyData>();
        var currentDate = timeRange.StartDate.Date;
        
        while (currentDate < timeRange.EndDate.Date)
        {
            var nextDate = currentDate.AddDays(1);
            var daySessions = userSessions.Where(u => u.EnqueuedAt.Date == currentDate).ToList();
            
            dailyData.Add(new DailyData(
                currentDate,
                daySessions.Count,
                daySessions.Count(u => u.Status == Domain.Enums.QueueStatus.Released),
                daySessions.Any() ? daySessions.Where(u => u.ReleasedAt.HasValue).Average(u => (u.ReleasedAt!.Value - u.EnqueuedAt).TotalMinutes) : 0,
                daySessions.Count(u => u.Status == Domain.Enums.QueueStatus.Waiting),
                4.2 // Mock satisfaction score
            ));
            
            currentDate = nextDate;
        }
        
        return dailyData;
    }

    private double CalculateAverageWaitTime(List<Domain.Entities.Queue> queues)
    {
        // Mock calculation - would need actual data
        return 15.5;
    }

    private double CalculateSystemEfficiency(List<Domain.Entities.Queue> queues)
    {
        // Mock calculation - would need actual data
        return 87.3;
    }

    private int GetUsersServedToday(Guid queueId, DateTime today)
    {
        // Mock data - would need actual query
        return 25;
    }

    private double GetAverageWaitTime(Guid queueId)
    {
        // Mock data - would need actual query
        return 12.3;
    }

    private async Task<List<RecentActivity>> GetRecentActivityAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        // Mock recent activity - would need actual data
        return new List<RecentActivity>
        {
            new RecentActivity(
                Guid.NewGuid().ToString(),
                "user_enqueued",
                "User 'customer123' joined queue",
                DateTime.UtcNow.AddMinutes(-5),
                "customer123",
                Guid.NewGuid(),
                "Main Queue"
            ),
            new RecentActivity(
                Guid.NewGuid().ToString(),
                "user_released",
                "User 'customer456' was released",
                DateTime.UtcNow.AddMinutes(-3),
                "customer456",
                Guid.NewGuid(),
                "Main Queue"
            )
        };
    }
}
