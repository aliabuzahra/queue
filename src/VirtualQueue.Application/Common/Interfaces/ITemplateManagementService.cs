namespace VirtualQueue.Application.Common.Interfaces;

public interface ITemplateManagementService
{
    // Methods expected by controllers
    Task<TemplateDto> CreateTemplateAsync(Guid tenantId, string name, string description, CancellationToken cancellationToken = default);
    Task<TemplateDto?> GetTemplateAsync(Guid templateId, CancellationToken cancellationToken = default);
    Task<TemplateDto?> GetTemplateByNameAndTypeAsync(string name, string type, CancellationToken cancellationToken = default);
    Task<List<TemplateDto>> GetAllTemplatesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TemplateDto> UpdateTemplateAsync(Guid templateId, string name, string description, CancellationToken cancellationToken = default);
    Task<bool> DeleteTemplateAsync(Guid templateId, CancellationToken cancellationToken = default);
    Task<string> RenderTemplateAsync(Guid templateId, Dictionary<string, object> variables, CancellationToken cancellationToken = default);
    
    // Original methods
    Task<TemplateDto> CreateTemplateAsync(CreateTemplateRequest request, CancellationToken cancellationToken = default);
    Task<TemplateDto> UpdateTemplateAsync(Guid templateId, UpdateTemplateRequest request, CancellationToken cancellationToken = default);
    Task<List<TemplateDto>> GetTemplatesAsync(Guid tenantId, TemplateType? type = null, CancellationToken cancellationToken = default);
    Task<TemplateDto> GetDefaultTemplateAsync(Guid tenantId, TemplateType type, CancellationToken cancellationToken = default);
    Task<string> RenderTemplateAsync(string templateContent, Dictionary<string, object> variables, CancellationToken cancellationToken = default);
    Task<bool> ValidateTemplateAsync(string templateContent, CancellationToken cancellationToken = default);
    Task<List<TemplateVariable>> GetTemplateVariablesAsync(Guid templateId, CancellationToken cancellationToken = default);
}

public record TemplateDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Description,
    TemplateType Type,
    string Subject,
    string Body,
    string? HtmlBody,
    List<TemplateVariable> Variables,
    bool IsDefault,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? CreatedBy
);

public record CreateTemplateRequest(
    Guid TenantId,
    string Name,
    string Description,
    TemplateType Type,
    string Subject,
    string Body,
    string? HtmlBody = null,
    bool IsDefault = false,
    bool IsActive = true,
    string? CreatedBy = null
);

public record UpdateTemplateRequest(
    string? Name,
    string? Description,
    string? Subject,
    string? Body,
    string? HtmlBody,
    bool? IsDefault,
    bool? IsActive
);

public record TemplateVariable(
    string Name,
    string Description,
    string Type,
    bool IsRequired,
    object? DefaultValue
);

public enum TemplateType
{
    Email,
    Sms,
    WhatsApp,
    PushNotification,
    Webhook
}
