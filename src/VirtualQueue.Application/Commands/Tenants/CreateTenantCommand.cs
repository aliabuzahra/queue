using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Commands.Tenants;

public record CreateTenantCommand(string Name, string Domain) : IRequest<TenantDto>;
