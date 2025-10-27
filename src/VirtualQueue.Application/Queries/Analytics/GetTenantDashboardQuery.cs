using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Analytics;

public record GetTenantDashboardQuery(
    Guid TenantId
) : IRequest<DashboardDto>;
