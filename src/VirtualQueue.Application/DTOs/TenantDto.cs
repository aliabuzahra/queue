namespace VirtualQueue.Application.DTOs;

public record TenantDto(
    Guid Id,
    string Name,
    string Domain,
    string ApiKey,
    bool IsActive,
    DateTime CreatedAt
);
