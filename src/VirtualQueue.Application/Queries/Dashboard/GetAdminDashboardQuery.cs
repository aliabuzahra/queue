using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Dashboard;

public record GetAdminDashboardQuery(
    Guid TenantId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    List<string>? Metrics = null) : IRequest<AdminDashboardDto>;

