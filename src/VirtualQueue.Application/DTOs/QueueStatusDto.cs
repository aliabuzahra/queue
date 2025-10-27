namespace VirtualQueue.Application.DTOs;

public record QueueStatusDto(
    Guid QueueId,
    string UserIdentifier,
    int Position,
    int TotalWaitingUsers,
    int TotalServingUsers,
    int TotalReleasedUsers,
    DateTime EnqueuedAt,
    string Status
);
