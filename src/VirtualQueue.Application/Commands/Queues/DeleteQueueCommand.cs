using MediatR;

namespace VirtualQueue.Application.Commands.Queues;

public record DeleteQueueCommand(Guid TenantId, Guid QueueId) : IRequest<bool>;
