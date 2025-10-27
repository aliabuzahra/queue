namespace VirtualQueue.Application.DTOs;

public record QueueDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Description,
    int MaxConcurrentUsers,
    int ReleaseRatePerMinute,
    bool IsActive,
    DateTime CreatedAt
);
