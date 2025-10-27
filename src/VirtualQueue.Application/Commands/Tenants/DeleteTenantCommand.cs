using MediatR;

namespace VirtualQueue.Application.Commands.Tenants;

public record DeleteTenantCommand(Guid Id) : IRequest<bool>;
