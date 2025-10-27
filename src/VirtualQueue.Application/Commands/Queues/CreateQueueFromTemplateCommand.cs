using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Commands.Queues;

public record CreateQueueFromTemplateCommand(
    Guid TenantId,
    Guid TemplateId,
    string QueueName,
    string? Description = null) : IRequest<QueueDto>;

