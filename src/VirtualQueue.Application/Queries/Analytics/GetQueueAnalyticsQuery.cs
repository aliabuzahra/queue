using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Analytics;

public record GetQueueAnalyticsQuery(
    Guid TenantId,
    Guid QueueId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<AdvancedAnalyticsDto>;