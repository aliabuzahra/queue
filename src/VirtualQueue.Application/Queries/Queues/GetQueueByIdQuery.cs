using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Queues;

public record GetQueueByIdQuery(Guid TenantId, Guid QueueId) : IRequest<QueueDto?>;
