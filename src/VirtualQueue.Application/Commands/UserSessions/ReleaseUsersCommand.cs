using MediatR;

namespace VirtualQueue.Application.Commands.UserSessions;

public record ReleaseUsersCommand(Guid TenantId, Guid QueueId, int Count) : IRequest<int>;
