namespace VirtualQueue.Application.DTOs;

public record AdminDashboardDto(
    Guid TenantId,
    DashboardOverview Overview,
    List<DashboardWidget> Widgets,
    List<DashboardChart> Charts,
    List<DashboardAlert> Alerts,
    DashboardSettings Settings,
    DateTime GeneratedAt);

public record DashboardOverview(
    int TotalQueues,
    int ActiveQueues,
    int TotalUsers,
    int ActiveUsers,
    int TotalTemplates,
    int PublicTemplates,
    double SystemHealth,
    DateTime LastUpdated);

public record DashboardWidget(
    string Id,
    string Title,
    string Type,
    object Data,
    DashboardWidgetPosition Position,
    DashboardWidgetSize Size,
    bool IsVisible);

public record DashboardChart(
    string Id,
    string Title,
    string ChartType,
    List<ChartDataPoint> DataPoints,
    ChartConfiguration Configuration);

public record ChartDataPoint(
    string Label,
    double Value,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = null);

public record ChartConfiguration(
    string XAxisLabel,
    string YAxisLabel,
    string ColorScheme,
    bool ShowLegend,
    bool ShowGrid,
    Dictionary<string, object>? CustomOptions = null);

public record DashboardAlert(
    string Id,
    string Title,
    string Message,
    string Severity,
    string Type,
    DateTime CreatedAt,
    bool IsRead,
    string? ActionUrl = null);

public record DashboardSettings(
    string Theme,
    string Layout,
    List<string> VisibleWidgets,
    Dictionary<string, object> CustomSettings);

public record DashboardWidgetPosition(
    int Row,
    int Column,
    int RowSpan = 1,
    int ColumnSpan = 1);

public record DashboardWidgetSize(
    int Width,
    int Height,
    string SizeUnit = "px");

public record DashboardDataRequest(
    Guid TenantId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    List<string>? Metrics = null,
    string? TimeRange = null);

public record DashboardExportRequest(
    Guid TenantId,
    string Format,
    List<string>? Widgets = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null);

