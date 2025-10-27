namespace VirtualQueue.Application.DTOs;

public record AdvancedAnalyticsDto(
    Guid QueueId,
    string QueueName,
    DateTime GeneratedAt,
    TimeRange TimeRange,
    QueueMetrics Metrics,
    UserMetrics UserMetrics,
    PerformanceMetrics Performance,
    TrendAnalysis Trends,
    List<HourlyData> HourlyBreakdown,
    List<DailyData> DailyBreakdown
);

public record TimeRange(
    DateTime StartDate,
    DateTime EndDate,
    string TimeZone
);

public record QueueMetrics(
    int TotalUsers,
    int WaitingUsers,
    int ServingUsers,
    int ReleasedUsers,
    int DroppedUsers,
    int AverageQueueLength,
    int PeakQueueLength,
    double AverageWaitTimeMinutes,
    double AverageServingTimeMinutes,
    int TotalReleases,
    int ReleasesToday,
    DateTime? LastReleaseAt
);

public record UserMetrics(
    int NewUsersToday,
    int ReturningUsers,
    int VIPUsers,
    int PremiumUsers,
    double UserSatisfactionScore,
    int NoShowCount,
    double NoShowRate
);

public record PerformanceMetrics(
    double ThroughputPerHour,
    double PeakThroughput,
    double AverageProcessingTime,
    double QueueEfficiency,
    double ResourceUtilization,
    int BottlenecksDetected
);

public record TrendAnalysis(
    QueueTrend QueueLengthTrend,
    WaitTimeTrend WaitTimeTrend,
    ThroughputTrend ThroughputTrend,
    UserSatisfactionTrend SatisfactionTrend
);

public record QueueTrend(
    string Direction, // "increasing", "decreasing", "stable"
    double PercentageChange,
    string Description
);

public record WaitTimeTrend(
    string Direction,
    double PercentageChange,
    double AverageChangeMinutes,
    string Description
);

public record ThroughputTrend(
    string Direction,
    double PercentageChange,
    int UsersPerHourChange,
    string Description
);

public record UserSatisfactionTrend(
    string Direction,
    double PercentageChange,
    double ScoreChange,
    string Description
);

public record HourlyData(
    int Hour,
    int UsersEnqueued,
    int UsersReleased,
    int AverageWaitTime,
    int QueueLength,
    double Throughput
);

public record DailyData(
    DateTime Date,
    int TotalUsers,
    int ReleasedUsers,
    double AverageWaitTime,
    int PeakQueueLength,
    double SatisfactionScore
);
