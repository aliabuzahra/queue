using VirtualQueue.Domain.Enums;

namespace VirtualQueue.Application.DTOs;

public record EnqueueUserRequest(
    string UserIdentifier,
    string? Metadata = null,
    QueuePriority Priority = QueuePriority.Normal
);
