using MediatR;
using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Commands.Queues;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Application.Queries.Queues;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId}/queue-templates")]
public class QueueTemplatesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<QueueTemplatesController> _logger;

    public QueueTemplatesController(IMediator mediator, ILogger<QueueTemplatesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public Task<ActionResult<QueueTemplateDto>> CreateTemplate(Guid tenantId, [FromBody] CreateQueueTemplateRequest request)
    {
        try
        {
            // In a real implementation, you would have a CreateQueueTemplateCommand
            _logger.LogInformation("Creating queue template for tenant {TenantId}: {TemplateName}", tenantId, request.Name);
            
            // Mock implementation
            var template = new QueueTemplateDto(
                Guid.NewGuid(),
                tenantId,
                request.Name,
                request.Description,
                request.TemplateType,
                request.MaxConcurrentUsers,
                request.ReleaseRatePerMinute,
                request.ScheduleJson,
                request.BusinessRulesJson,
                request.NotificationSettingsJson,
                request.IsPublic,
                true,
                0,
                new Dictionary<string, string>(),
                DateTime.UtcNow,
                null
            );

            return Task.FromResult<ActionResult<QueueTemplateDto>>(CreatedAtAction(nameof(GetTemplate), new { tenantId, templateId = template.Id }, template));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating queue template for tenant {TenantId}", tenantId);
            return Task.FromResult<ActionResult<QueueTemplateDto>>(StatusCode(500, new { message = "Template creation error" }));
        }
    }

    [HttpGet("{templateId}")]
    public Task<ActionResult<QueueTemplateDto>> GetTemplate(Guid tenantId, Guid templateId)
    {
        try
        {
            _logger.LogInformation("Getting queue template {TemplateId} for tenant {TenantId}", templateId, tenantId);
            
            // Mock implementation
            var template = new QueueTemplateDto(
                templateId,
                tenantId,
                "Sample Template",
                "A sample queue template",
                "CustomerService",
                100,
                10,
                null,
                null,
                null,
                false,
                true,
                5,
                new Dictionary<string, string>(),
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow.AddDays(-1)
            );

            return Task.FromResult<ActionResult<QueueTemplateDto>>(Ok(template));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving queue template {TemplateId} for tenant {TenantId}", templateId, tenantId);
            return Task.FromResult<ActionResult<QueueTemplateDto>>(StatusCode(500, new { message = "Template retrieval error" }));
        }
    }

    [HttpGet]
    public Task<ActionResult<List<QueueTemplateDto>>> GetTemplates(Guid tenantId)
    {
        try
        {
            _logger.LogInformation("Getting queue templates for tenant {TenantId}", tenantId);
            
            // Mock implementation
            var templates = new List<QueueTemplateDto>
            {
                new QueueTemplateDto(
                    Guid.NewGuid(),
                    tenantId,
                    "Customer Service Template",
                    "Template for customer service queues",
                    "CustomerService",
                    50,
                    5,
                    null,
                    null,
                    null,
                    true,
                    true,
                    10,
                    new Dictionary<string, string>(),
                    DateTime.UtcNow.AddDays(-10),
                    DateTime.UtcNow.AddDays(-2)
                ),
                new QueueTemplateDto(
                    Guid.NewGuid(),
                    tenantId,
                    "Support Template",
                    "Template for technical support queues",
                    "Support",
                    30,
                    3,
                    null,
                    null,
                    null,
                    false,
                    true,
                    5,
                    new Dictionary<string, string>(),
                    DateTime.UtcNow.AddDays(-5),
                    null
                )
            };

            return Task.FromResult<ActionResult<List<QueueTemplateDto>>>(Ok(templates));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving queue templates for tenant {TenantId}", tenantId);
            return Task.FromResult<ActionResult<List<QueueTemplateDto>>>(StatusCode(500, new { message = "Templates retrieval error" }));
        }
    }

    [HttpPost("{templateId}/create-queue")]
    public async Task<ActionResult<QueueDto>> CreateQueueFromTemplate(
        Guid tenantId, 
        Guid templateId, 
        [FromBody] CreateQueueFromTemplateRequest request)
    {
        try
        {
            var command = new CreateQueueFromTemplateCommand(tenantId, templateId, request.QueueName, request.Description);
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Queue created from template {TemplateId} for tenant {TenantId}: {QueueId}", 
                templateId, tenantId, result.Id);
            
            return Created($"/api/tenants/{tenantId}/queues/{result.Id}", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating queue from template {TemplateId} for tenant {TenantId}", templateId, tenantId);
            return StatusCode(500, new { message = "Queue creation from template error" });
        }
    }
}

public record CreateQueueFromTemplateRequest(string QueueName, string? Description = null);

