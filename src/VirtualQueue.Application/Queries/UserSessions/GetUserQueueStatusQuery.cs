using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.UserSessions;

public record GetUserQueueStatusQuery(Guid TenantId, Guid QueueId, string UserIdentifier) : IRequest<QueueStatusDto?>;
