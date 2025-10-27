using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Queues;

public record GetQueuesByTenantIdQuery(Guid TenantId) : IRequest<IEnumerable<QueueDto>>;
