using MediatR;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Domain.Enums;

namespace VirtualQueue.Application.Commands.UserSessions;

public record EnqueueUserCommand(
    Guid TenantId,
    Guid QueueId,
    string UserIdentifier,
    string? Metadata = null,
    QueuePriority Priority = QueuePriority.Normal
) : IRequest<UserSessionDto>;
