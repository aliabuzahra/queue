using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Infrastructure.Services;

public class TemplateManagementService : ITemplateManagementService
{
    private readonly ILogger<TemplateManagementService> _logger;
    private readonly ICacheService _cacheService;

    public TemplateManagementService(ILogger<TemplateManagementService> logger, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<TemplateDto> CreateTemplateAsync(CreateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var templateId = Guid.NewGuid();
            var template = new TemplateDto(
                templateId,
                request.TenantId,
                request.Name,
                request.Description,
                request.Type,
                request.Subject,
                request.Body,
                request.HtmlBody,
                ExtractVariables(request.Body),
                request.IsDefault,
                request.IsActive,
                DateTime.UtcNow,
                DateTime.UtcNow,
                request.CreatedBy
            );

            var cacheKey = $"template:{request.TenantId}:{templateId}";
            await _cacheService.SetAsync(cacheKey, template, TimeSpan.FromDays(365), cancellationToken);

            _logger.LogInformation("Template created: {TemplateId} for tenant {TenantId}", templateId, request.TenantId);
            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create template for tenant {TenantId}", request.TenantId);
            throw;
        }
    }

    public async Task<TemplateDto> UpdateTemplateAsync(Guid templateId, UpdateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would update the database
            _logger.LogInformation("Template updated: {TemplateId}", templateId);
            
            // For now, return a mock updated template
            return new TemplateDto(
                templateId,
                Guid.NewGuid(),
                request.Name ?? "Updated Template",
                request.Description ?? "Updated Description",
                TemplateType.Email,
                request.Subject ?? "Updated Subject",
                request.Body ?? "Updated Body",
                request.HtmlBody,
                new List<TemplateVariable>(),
                request.IsDefault ?? false,
                request.IsActive ?? true,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow,
                "System"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update template {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<bool> DeleteTemplateAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Template deleted: {TemplateId}", templateId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete template {TemplateId}", templateId);
            return false;
        }
    }

    public async Task<TemplateDto?> GetTemplateAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would query the database
            _logger.LogInformation("Retrieving template: {TemplateId}", templateId);
            
            return new TemplateDto(
                templateId,
                Guid.NewGuid(),
                "Sample Template",
                "A sample template for testing",
                TemplateType.Email,
                "Welcome to {{queue_name}}",
                "Hello {{user_name}}, you are position {{position}} in {{queue_name}}",
                "<h1>Welcome to {{queue_name}}</h1><p>Hello {{user_name}}, you are position {{position}} in {{queue_name}}</p>",
                new List<TemplateVariable>
                {
                    new("user_name", "User's name", "string", true, null),
                    new("queue_name", "Queue name", "string", true, null),
                    new("position", "User's position", "number", true, null)
                },
                true,
                true,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow,
                "System"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get template {TemplateId}", templateId);
            return null;
        }
    }

    public async Task<List<TemplateDto>> GetTemplatesAsync(Guid tenantId, TemplateType? type = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting templates for tenant {TenantId}, type: {Type}", tenantId, type);
            
            // In a real implementation, this would query the database
            return new List<TemplateDto>
            {
                new TemplateDto(
                    Guid.NewGuid(),
                    tenantId,
                    "Welcome Email",
                    "Welcome email template",
                    TemplateType.Email,
                    "Welcome to {{queue_name}}",
                    "Hello {{user_name}}, welcome to {{queue_name}}",
                    "<h1>Welcome to {{queue_name}}</h1>",
                    new List<TemplateVariable>(),
                    true,
                    true,
                    DateTime.UtcNow.AddDays(-30),
                    DateTime.UtcNow,
                    "System"
                )
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get templates for tenant {TenantId}", tenantId);
            return new List<TemplateDto>();
        }
    }

    public async Task<TemplateDto> GetDefaultTemplateAsync(Guid tenantId, TemplateType type, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting default template for tenant {TenantId}, type: {Type}", tenantId, type);
            
            return new TemplateDto(
                Guid.NewGuid(),
                tenantId,
                $"Default {type} Template",
                $"Default template for {type}",
                type,
                $"Default {type} Subject",
                $"Default {type} Body",
                null,
                new List<TemplateVariable>(),
                true,
                true,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow,
                "System"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get default template for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<string> RenderTemplateAsync(Guid templateId, Dictionary<string, object> variables, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await GetTemplateAsync(templateId, cancellationToken);
            if (template == null)
            {
                throw new InvalidOperationException($"Template {templateId} not found");
            }

            return await RenderTemplateAsync(template.Body, variables, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<string> RenderTemplateAsync(string templateContent, Dictionary<string, object> variables, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = templateContent;
            
            // Simple template variable replacement
            foreach (var variable in variables)
            {
                var placeholder = $"{{{{{variable.Key}}}}}";
                result = result.Replace(placeholder, variable.Value?.ToString() ?? "");
            }

            _logger.LogInformation("Template rendered successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template content");
            throw;
        }
    }

    public async Task<bool> ValidateTemplateAsync(string templateContent, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check for balanced braces
            var openBraces = templateContent.Count(c => c == '{');
            var closeBraces = templateContent.Count(c => c == '}');
            
            if (openBraces != closeBraces)
            {
                return false;
            }

            // Check for valid variable syntax
            var variablePattern = @"\{\{([^}]+)\}\}";
            var matches = Regex.Matches(templateContent, variablePattern);
            
            foreach (Match match in matches)
            {
                var variableName = match.Groups[1].Value.Trim();
                if (string.IsNullOrWhiteSpace(variableName))
                {
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate template content");
            return false;
        }
    }

    public async Task<List<TemplateVariable>> GetTemplateVariablesAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await GetTemplateAsync(templateId, cancellationToken);
            if (template == null)
            {
                return new List<TemplateVariable>();
            }

            return ExtractVariables(template.Body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get template variables for {TemplateId}", templateId);
            return new List<TemplateVariable>();
        }
    }

    // New methods required by the interface
    public async Task<TemplateDto> CreateTemplateAsync(Guid tenantId, string name, string description, CancellationToken cancellationToken = default)
    {
        try
        {
            var templateId = Guid.NewGuid();
            var template = new TemplateDto(
                templateId,
                tenantId,
                name,
                description,
                TemplateType.Email,
                "Default Subject",
                "Default Body",
                null,
                new List<TemplateVariable>(),
                false,
                true,
                DateTime.UtcNow,
                DateTime.UtcNow,
                "System"
            );

            var cacheKey = $"template:{tenantId}:{templateId}";
            await _cacheService.SetAsync(cacheKey, template, TimeSpan.FromDays(365), cancellationToken);

            _logger.LogInformation("Template created: {TemplateId} for tenant {TenantId}", templateId, tenantId);
            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create template for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<TemplateDto?> GetTemplateByNameAndTypeAsync(string name, string type, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting template by name {Name} and type {Type}", name, type);
            
            // In a real implementation, this would query the database
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get template by name {Name} and type {Type}", name, type);
            return null;
        }
    }

    public async Task<List<TemplateDto>> GetAllTemplatesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting all templates for tenant {TenantId}", tenantId);
            
            // In a real implementation, this would query the database
            return await GetTemplatesAsync(tenantId, null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all templates for tenant {TenantId}", tenantId);
            return new List<TemplateDto>();
        }
    }

    public async Task<TemplateDto> UpdateTemplateAsync(Guid templateId, string name, string description, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Template updated: {TemplateId}", templateId);
            
            var template = new TemplateDto(
                templateId,
                Guid.NewGuid(),
                name,
                description,
                TemplateType.Email,
                "Updated Subject",
                "Updated Body",
                null,
                new List<TemplateVariable>(),
                false,
                true,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow,
                "System"
            );

            var cacheKey = $"template:{template.TenantId}:{templateId}";
            await _cacheService.SetAsync(cacheKey, template, TimeSpan.FromDays(365), cancellationToken);

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update template {TemplateId}", templateId);
            throw;
        }
    }

    private List<TemplateVariable> ExtractVariables(string templateContent)
    {
        var variables = new List<TemplateVariable>();
        var variablePattern = @"\{\{([^}]+)\}\}";
        var matches = Regex.Matches(templateContent, variablePattern);
        
        var uniqueVariables = new HashSet<string>();
        
        foreach (Match match in matches)
        {
            var variableName = match.Groups[1].Value.Trim();
            if (!uniqueVariables.Contains(variableName))
            {
                uniqueVariables.Add(variableName);
                variables.Add(new TemplateVariable(
                    variableName,
                    $"Variable: {variableName}",
                    "string",
                    true,
                    null
                ));
            }
        }

        return variables;
    }
}
