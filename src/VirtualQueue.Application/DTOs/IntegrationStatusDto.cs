namespace VirtualQueue.Application.DTOs;

/// <summary>
/// Data transfer object for integration status.
/// </summary>
public record IntegrationStatusDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Type,
    string Status,
    DateTime LastSyncAt,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Request model for creating an integration.
/// </summary>
public record CreateIntegrationRequest(
    string Name,
    string Type,
    string Configuration
);

/// <summary>
/// Request model for updating an integration.
/// </summary>
public record UpdateIntegrationRequest(
    string Name,
    string Type,
    string Configuration
);
