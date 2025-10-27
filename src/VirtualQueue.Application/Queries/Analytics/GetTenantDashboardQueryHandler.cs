using MediatR;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Analytics;

public class GetTenantDashboardQueryHandler : IRequestHandler<GetTenantDashboardQuery, DashboardDto>
{
    private readonly IAnalyticsService _analyticsService;

    public GetTenantDashboardQueryHandler(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public async Task<DashboardDto> Handle(GetTenantDashboardQuery request, CancellationToken cancellationToken)
    {
        return await _analyticsService.GetTenantDashboardAsync(request.TenantId, cancellationToken);
    }
}
