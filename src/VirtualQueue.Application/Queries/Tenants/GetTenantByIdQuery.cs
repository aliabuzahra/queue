using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Tenants;

public record GetTenantByIdQuery(Guid Id) : IRequest<TenantDto?>;
