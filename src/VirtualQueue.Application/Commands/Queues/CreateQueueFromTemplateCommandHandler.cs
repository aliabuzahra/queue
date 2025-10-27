using MediatR;
using Microsoft.Extensions.Logging;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Commands.Queues;

public class CreateQueueFromTemplateCommandHandler : IRequestHandler<CreateQueueFromTemplateCommand, QueueDto>
{
    private readonly IQueueTemplateService _templateService;
    private readonly ILogger<CreateQueueFromTemplateCommandHandler> _logger;

    public CreateQueueFromTemplateCommandHandler(
        IQueueTemplateService templateService,
        ILogger<CreateQueueFromTemplateCommandHandler> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<QueueDto> Handle(CreateQueueFromTemplateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating queue from template {TemplateId} for tenant {TenantId}", 
            request.TemplateId, request.TenantId);

        var templateUsageRequest = new QueueTemplateUsageRequest(
            request.TemplateId,
            request.QueueName,
            request.Description);

        var queue = await _templateService.CreateQueueFromTemplateAsync(
            request.TenantId,
            templateUsageRequest,
            cancellationToken);

        _logger.LogInformation("Queue created from template: {QueueId}", queue.Id);
        return queue;
    }
}

