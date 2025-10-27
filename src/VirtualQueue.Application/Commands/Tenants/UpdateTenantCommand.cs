using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Commands.Tenants;

public record UpdateTenantCommand(Guid Id, string Name, string Domain) : IRequest<TenantDto>;
