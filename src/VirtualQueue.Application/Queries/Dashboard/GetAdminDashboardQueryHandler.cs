using MediatR;
using Microsoft.Extensions.Logging;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Dashboard;

public class GetAdminDashboardQueryHandler : IRequestHandler<GetAdminDashboardQuery, AdminDashboardDto>
{
    private readonly IQueueRepository _queueRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IQueueTemplateService _templateService;
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetAdminDashboardQueryHandler> _logger;

    public GetAdminDashboardQueryHandler(
        IQueueRepository queueRepository,
        IUserSessionRepository userSessionRepository,
        IQueueTemplateService templateService,
        IAnalyticsService analyticsService,
        ILogger<GetAdminDashboardQueryHandler> logger)
    {
        _queueRepository = queueRepository;
        _userSessionRepository = userSessionRepository;
        _templateService = templateService;
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<AdminDashboardDto> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating admin dashboard for tenant {TenantId}", request.TenantId);

        // Get basic statistics
        var queues = await _queueRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        var activeQueues = queues.Count(q => q.IsActive);
        var totalUsers = await _userSessionRepository.GetUserCountByDateRangeAsync(
            request.TenantId, 
            request.StartDate ?? DateTime.UtcNow.AddDays(-30), 
            request.EndDate ?? DateTime.UtcNow, 
            cancellationToken);

        // Get template statistics
        var templateStats = await _templateService.GetTemplateStatisticsAsync(request.TenantId, cancellationToken);

        // Create overview
        var overview = new DashboardOverview(
            TotalQueues: queues.Count(),
            ActiveQueues: activeQueues,
            TotalUsers: totalUsers,
            ActiveUsers: totalUsers, // Mock data
            TotalTemplates: templateStats.GetValueOrDefault("Total", 0),
            PublicTemplates: templateStats.GetValueOrDefault("Public", 0),
            SystemHealth: CalculateSystemHealth(queues.Count(), activeQueues),
            LastUpdated: DateTime.UtcNow
        );

        // Create widgets
        var widgets = CreateDashboardWidgets(request.TenantId, overview);

        // Create charts
        var charts = await CreateDashboardCharts(request.TenantId, request.StartDate, request.EndDate, cancellationToken);

        // Create alerts
        var alerts = await CreateDashboardAlerts(request.TenantId, cancellationToken);

        // Create settings
        var settings = new DashboardSettings(
            Theme: "light",
            Layout: "grid",
            VisibleWidgets: widgets.Select(w => w.Id).ToList(),
            CustomSettings: new Dictionary<string, object>()
        );

        return new AdminDashboardDto(
            request.TenantId,
            overview,
            widgets,
            charts,
            alerts,
            settings,
            DateTime.UtcNow
        );
    }

    private List<DashboardWidget> CreateDashboardWidgets(Guid tenantId, DashboardOverview overview)
    {
        return new List<DashboardWidget>
        {
            new DashboardWidget(
                "queue-stats",
                "Queue Statistics",
                "statistics",
                overview,
                new DashboardWidgetPosition(0, 0, 2, 1),
                new DashboardWidgetSize(400, 200),
                true
            ),
            new DashboardWidget(
                "user-activity",
                "User Activity",
                "chart",
                new { ActiveUsers = overview.ActiveUsers, TotalUsers = overview.TotalUsers },
                new DashboardWidgetPosition(0, 1, 2, 1),
                new DashboardWidgetSize(400, 200),
                true
            ),
            new DashboardWidget(
                "system-health",
                "System Health",
                "gauge",
                new { Health = overview.SystemHealth, Status = overview.SystemHealth > 80 ? "Good" : "Warning" },
                new DashboardWidgetPosition(2, 0, 1, 1),
                new DashboardWidgetSize(200, 200),
                true
            )
        };
    }

    private async Task<List<DashboardChart>> CreateDashboardCharts(
        Guid tenantId, 
        DateTime? startDate, 
        DateTime? endDate, 
        CancellationToken cancellationToken)
    {
        var charts = new List<DashboardChart>();

        // Queue usage chart
        var queueUsageData = await GenerateQueueUsageData(tenantId, startDate, endDate, cancellationToken);
        charts.Add(new DashboardChart(
            "queue-usage",
            "Queue Usage Over Time",
            "line",
            queueUsageData,
            new ChartConfiguration("Time", "Users", "blue", true, true)
        ));

        // User activity chart
        var userActivityData = await GenerateUserActivityData(tenantId, startDate, endDate, cancellationToken);
        charts.Add(new DashboardChart(
            "user-activity",
            "User Activity",
            "bar",
            userActivityData,
            new ChartConfiguration("Hour", "Active Users", "green", true, true)
        ));

        return charts;
    }

    private async Task<List<DashboardAlert>> CreateDashboardAlerts(Guid tenantId, CancellationToken cancellationToken)
    {
        // Mock alerts - in real implementation, these would come from the alert system
        return new List<DashboardAlert>
        {
            new DashboardAlert(
                Guid.NewGuid().ToString(),
                "High Queue Load",
                "Queue 'Customer Service' has reached 90% capacity",
                "warning",
                "queue",
                DateTime.UtcNow.AddMinutes(-30),
                false,
                "/queues/customer-service"
            ),
            new DashboardAlert(
                Guid.NewGuid().ToString(),
                "System Update Available",
                "A new system update is available for installation",
                "info",
                "system",
                DateTime.UtcNow.AddHours(-2),
                false,
                "/admin/updates"
            )
        };
    }

    private double CalculateSystemHealth(int totalQueues, int activeQueues)
    {
        if (totalQueues == 0) return 100.0;
        return (double)activeQueues / totalQueues * 100.0;
    }

    private async Task<List<ChartDataPoint>> GenerateQueueUsageData(
        Guid tenantId, 
        DateTime? startDate, 
        DateTime? endDate, 
        CancellationToken cancellationToken)
    {
        // Mock data - in real implementation, this would query actual usage data
        var dataPoints = new List<ChartDataPoint>();
        var currentDate = startDate ?? DateTime.UtcNow.AddDays(-7);
        var end = endDate ?? DateTime.UtcNow;

        for (var date = currentDate; date <= end; date = date.AddHours(1))
        {
            dataPoints.Add(new ChartDataPoint(
                date.ToString("HH:mm"),
                Random.Shared.Next(10, 100),
                date
            ));
        }

        return dataPoints;
    }

    private async Task<List<ChartDataPoint>> GenerateUserActivityData(
        Guid tenantId, 
        DateTime? startDate, 
        DateTime? endDate, 
        CancellationToken cancellationToken)
    {
        // Mock data - in real implementation, this would query actual user activity
        var dataPoints = new List<ChartDataPoint>();
        var currentDate = startDate ?? DateTime.UtcNow.AddDays(-1);
        var end = endDate ?? DateTime.UtcNow;

        for (var date = currentDate; date <= end; date = date.AddHours(1))
        {
            dataPoints.Add(new ChartDataPoint(
                date.ToString("HH:mm"),
                Random.Shared.Next(5, 50),
                date
            ));
        }

        return dataPoints;
    }
}

