using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId}/templates")]
public class TemplateManagementController : ControllerBase
{
    private readonly ITemplateManagementService _templateService;
    private readonly ILogger<TemplateManagementController> _logger;

    public TemplateManagementController(ITemplateManagementService templateService, ILogger<TemplateManagementController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<TemplateDto>> CreateTemplate(Guid tenantId, [FromBody] CreateTemplateRequest request)
    {
        try
        {
            var template = await _templateService.CreateTemplateAsync(
                new VirtualQueue.Application.Common.Interfaces.CreateTemplateRequest(
                    tenantId,
                    request.Name,
                    "", // No description available in API request
                    Enum.Parse<VirtualQueue.Application.Common.Interfaces.TemplateType>(request.Type),
                    "", // No subject available in API request
                    request.Content,
                    null, // No HTML content available in API request
                    false,
                    true
                ));
            
            _logger.LogInformation("Template created for tenant {TenantId}: {TemplateName}", tenantId, request.Name);
            return CreatedAtAction(nameof(GetTemplate), new { tenantId, templateId = template.Id }, template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Template creation error" });
        }
    }

    [HttpGet("{templateId}")]
    public async Task<ActionResult<TemplateDto>> GetTemplate(Guid tenantId, Guid templateId)
    {
        try
        {
            var template = await _templateService.GetTemplateAsync(templateId);
            if (template == null)
                return NotFound(new { message = "Template not found" });
            
            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template {TemplateId} for tenant {TenantId}", templateId, tenantId);
            return StatusCode(500, new { message = "Template retrieval error" });
        }
    }

    [HttpGet("by-name/{name}/{type}")]
    public async Task<ActionResult<TemplateDto>> GetTemplateByNameAndType(Guid tenantId, string name, string type)
    {
        try
        {
            var template = await _templateService.GetTemplateByNameAndTypeAsync(name, type);
            if (template == null)
                return NotFound(new { message = "Template not found" });
            
            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template {Name}/{Type} for tenant {TenantId}", name, type, tenantId);
            return StatusCode(500, new { message = "Template retrieval error" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<TemplateDto>>> GetAllTemplates(Guid tenantId)
    {
        try
        {
            var templates = await _templateService.GetAllTemplatesAsync(tenantId);
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving templates for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Templates retrieval error" });
        }
    }

    [HttpPut("{templateId}")]
    public async Task<ActionResult<TemplateDto>> UpdateTemplate(Guid tenantId, Guid templateId, [FromBody] UpdateTemplateRequest request)
    {
        try
        {
            var template = await _templateService.UpdateTemplateAsync(
                templateId,
                request.Name ?? "",
                request.Type ?? "");
            
            _logger.LogInformation("Template updated for tenant {TenantId}: {TemplateId}", tenantId, templateId);
            return Ok(template);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Template not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template {TemplateId} for tenant {TenantId}", templateId, tenantId);
            return StatusCode(500, new { message = "Template update error" });
        }
    }

    [HttpDelete("{templateId}")]
    public async Task<ActionResult> DeleteTemplate(Guid tenantId, Guid templateId)
    {
        try
        {
            await _templateService.DeleteTemplateAsync(templateId);
            _logger.LogInformation("Template deleted for tenant {TenantId}: {TemplateId}", tenantId, templateId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template {TemplateId} for tenant {TenantId}", templateId, tenantId);
            return StatusCode(500, new { message = "Template deletion error" });
        }
    }

    [HttpPost("render")]
    public async Task<ActionResult<TemplateRenderResponse>> RenderTemplate(Guid tenantId, [FromBody] RenderTemplateRequest request)
    {
        try
        {
            var renderedContent = await _templateService.RenderTemplateAsync(
                request.Name,
                request.Data.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value));
            
            var response = new TemplateRenderResponse(renderedContent, DateTime.UtcNow);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template {Name}/{Type} for tenant {TenantId}", request.Name, request.Type, tenantId);
            return StatusCode(500, new { message = "Template rendering error" });
        }
    }
}

public record CreateTemplateRequest(string Name, string Type, string Content);
public record UpdateTemplateRequest(string? Name, string? Type, string? Content);
public record RenderTemplateRequest(string Name, string Type, Dictionary<string, string> Data);
public record TemplateRenderResponse(string RenderedContent, DateTime RenderedAt);
