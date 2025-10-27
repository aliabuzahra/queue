using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Tenants;

public record GetAllTenantsQuery() : IRequest<IEnumerable<TenantDto>>;
