using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.UserSessions;

public record GetUserSessionsByQueueIdQuery(Guid TenantId, Guid QueueId) : IRequest<IEnumerable<UserSessionDto>>;
