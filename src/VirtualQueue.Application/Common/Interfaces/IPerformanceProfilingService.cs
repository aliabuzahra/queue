namespace VirtualQueue.Application.Common.Interfaces;

public interface IPerformanceProfilingService
{
    Task<ProfileDto> StartProfileAsync(string profileName, ProfileType type, CancellationToken cancellationToken = default);
    Task<ProfileDto> StopProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<ProfileDto?> GetProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<List<ProfileDto>> GetProfilesAsync(Guid tenantId, ProfileType? type = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<PerformanceReport> GeneratePerformanceReportAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<List<PerformanceBottleneck>> IdentifyBottlenecksAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<PerformanceRecommendation> GetPerformanceRecommendationsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> StartContinuousProfilingAsync(Guid tenantId, ContinuousProfilingConfig config, CancellationToken cancellationToken = default);
    Task<bool> StopContinuousProfilingAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<ProfilingMetrics> GetPerformanceMetricsAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

public record ProfileDto(
    Guid Id,
    Guid TenantId,
    string Name,
    ProfileType Type,
    ProfileStatus Status,
    DateTime StartTime,
    DateTime? EndTime,
    TimeSpan? Duration,
    ProfilingMetrics Metrics,
    Dictionary<string, object>? Metadata = null
);

public record PerformanceReport(
    Guid TenantId,
    DateTime GeneratedAt,
    ProfilingTimeRange TimeRange,
    OverallPerformance Overall,
    List<ComponentPerformance> Components,
    List<PerformanceBottleneck> Bottlenecks,
    List<PerformanceRecommendation> Recommendations,
    PerformanceTrends Trends
);

public record ComponentPerformance(
    string ComponentName,
    double AverageResponseTime,
    double MaxResponseTime,
    double MinResponseTime,
    int RequestCount,
    double ErrorRate,
    double Throughput,
    PerformanceStatus Status
);

public record PerformanceBottleneck(
    string ComponentName,
    string Description,
    BottleneckSeverity Severity,
    double Impact,
    string Recommendation,
    DateTime DetectedAt
);

public record PerformanceRecommendation(
    string Title,
    string Description,
    RecommendationType Type,
    double PotentialImprovement,
    string Implementation,
    int Priority
);

public record PerformanceTrends(
    Trend ResponseTimeTrend,
    Trend ThroughputTrend,
    Trend ErrorRateTrend,
    Trend ResourceUsageTrend
);

public record Trend(
    string Direction,
    double PercentageChange,
    string Description
);

public record ProfilingMetrics(
    double AverageResponseTime,
    double MaxResponseTime,
    double MinResponseTime,
    int TotalRequests,
    double ErrorRate,
    double Throughput,
    double CpuUsage,
    double MemoryUsage,
    double DatabaseUsage,
    DateTime MeasuredAt
);

public record ContinuousProfilingConfig(
    TimeSpan SamplingInterval,
    List<string> ComponentsToProfile,
    bool ProfileDatabase,
    bool ProfileCache,
    bool ProfileExternalServices
);

public record ProfilingTimeRange(
    DateTime StartDate,
    DateTime EndDate,
    string TimeZone
);

public enum ProfileType
{
    Request,
    Database,
    Cache,
    ExternalService,
    Custom
}

public enum ProfileStatus
{
    Running,
    Completed,
    Failed,
    Cancelled
}

public enum PerformanceStatus
{
    Excellent,
    Good,
    Fair,
    Poor,
    Critical
}

public enum BottleneckSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum RecommendationType
{
    Optimization,
    Scaling,
    Caching,
    Database,
    Infrastructure
}

public record OverallPerformance(
    PerformanceStatus Status,
    double AverageResponseTime,
    double Throughput,
    double ErrorRate,
    double Availability
);
