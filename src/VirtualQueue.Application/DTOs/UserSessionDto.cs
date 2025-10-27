using VirtualQueue.Domain.Enums;

namespace VirtualQueue.Application.DTOs;

public record UserSessionDto(
    Guid Id,
    Guid QueueId,
    string UserIdentifier,
    QueueStatus Status,
    QueuePriority Priority,
    DateTime EnqueuedAt,
    DateTime? ReleasedAt,
    DateTime? ServedAt,
    int Position,
    string? Metadata
);