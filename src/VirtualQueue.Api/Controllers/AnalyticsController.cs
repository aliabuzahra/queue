using MediatR;
using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Queries.Analytics;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId}/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("queues/{queueId}")]
    public async Task<ActionResult<AdvancedAnalyticsDto>> GetQueueAnalytics(
        Guid tenantId, 
        Guid queueId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetQueueAnalyticsQuery(tenantId, queueId, startDate, endDate);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetTenantDashboard(Guid tenantId)
    {
        var query = new GetTenantDashboardQuery(tenantId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("queues/{queueId}/hourly")]
    public async Task<ActionResult<List<HourlyData>>> GetHourlyAnalytics(
        Guid tenantId,
        Guid queueId,
        [FromQuery] DateTime date)
    {
        var query = new GetQueueAnalyticsQuery(tenantId, queueId, date.Date, date.Date.AddDays(1));
        var result = await _mediator.Send(query);
        return Ok(result.HourlyBreakdown);
    }

    [HttpGet("queues/{queueId}/daily")]
    public async Task<ActionResult<List<DailyData>>> GetDailyAnalytics(
        Guid tenantId,
        Guid queueId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var query = new GetQueueAnalyticsQuery(tenantId, queueId, startDate, endDate);
        var result = await _mediator.Send(query);
        return Ok(result.DailyBreakdown);
    }

    [HttpGet("queues/{queueId}/performance")]
    public async Task<ActionResult<PerformanceMetrics>> GetPerformanceMetrics(
        Guid tenantId,
        Guid queueId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetQueueAnalyticsQuery(tenantId, queueId, startDate, endDate);
        var result = await _mediator.Send(query);
        return Ok(result.Performance);
    }

    [HttpGet("queues/{queueId}/trends")]
    public async Task<ActionResult<TrendAnalysis>> GetTrendAnalysis(
        Guid tenantId,
        Guid queueId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetQueueAnalyticsQuery(tenantId, queueId, startDate, endDate);
        var result = await _mediator.Send(query);
        return Ok(result.Trends);
    }
}
