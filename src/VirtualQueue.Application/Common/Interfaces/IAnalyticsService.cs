using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Common.Interfaces;

public interface IAnalyticsService
{
    Task<AdvancedAnalyticsDto> GetQueueAnalyticsAsync(Guid tenantId, Guid queueId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<DashboardDto> GetTenantDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<HourlyData>> GetHourlyAnalyticsAsync(Guid tenantId, Guid queueId, DateTime date, CancellationToken cancellationToken = default);
    Task<List<DailyData>> GetDailyAnalyticsAsync(Guid tenantId, Guid queueId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<PerformanceMetrics> GetPerformanceMetricsAsync(Guid tenantId, Guid queueId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<TrendAnalysis> GetTrendAnalysisAsync(Guid tenantId, Guid queueId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<List<Alert>> GetAlertsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<SystemHealth> GetSystemHealthAsync(CancellationToken cancellationToken = default);
}
