using MediatR;

namespace VirtualQueue.Application.Commands.UserSessions;

public record RemoveUserCommand(Guid TenantId, Guid QueueId, string UserIdentifier) : IRequest<bool>;
