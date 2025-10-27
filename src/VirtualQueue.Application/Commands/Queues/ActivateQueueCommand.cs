using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Commands.Queues;

public record ActivateQueueCommand(Guid TenantId, Guid QueueId) : IRequest<QueueDto>;
