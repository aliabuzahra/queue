using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Commands.Queues;

public record SetQueueScheduleCommand(
    Guid TenantId,
    Guid QueueId,
    QueueScheduleDto Schedule
) : IRequest<QueueDto>;
