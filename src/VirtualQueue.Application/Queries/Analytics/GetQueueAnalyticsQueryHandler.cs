using MediatR;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Analytics;

public class GetQueueAnalyticsQueryHandler : IRequestHandler<GetQueueAnalyticsQuery, AdvancedAnalyticsDto>
{
    private readonly IAnalyticsService _analyticsService;

    public GetQueueAnalyticsQueryHandler(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public async Task<AdvancedAnalyticsDto> Handle(GetQueueAnalyticsQuery request, CancellationToken cancellationToken)
    {
        return await _analyticsService.GetQueueAnalyticsAsync(
            request.TenantId,
            request.QueueId,
            request.StartDate,
            request.EndDate,
            cancellationToken);
    }
}