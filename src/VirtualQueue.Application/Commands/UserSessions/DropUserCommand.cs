using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Commands.UserSessions;

public record DropUserCommand(Guid TenantId, Guid QueueId, string UserIdentifier) : IRequest<UserSessionDto?>;
