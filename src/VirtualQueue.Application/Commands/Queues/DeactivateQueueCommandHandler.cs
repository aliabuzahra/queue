using AutoMapper;
using MediatR;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Commands.Queues;

public class DeactivateQueueCommandHandler : IRequestHandler<DeactivateQueueCommand, QueueDto>
{
    private readonly IQueueRepository _queueRepository;
    private readonly IMapper _mapper;

    public DeactivateQueueCommandHandler(IQueueRepository queueRepository, IMapper mapper)
    {
        _queueRepository = queueRepository;
        _mapper = mapper;
    }

    public async Task<QueueDto> Handle(DeactivateQueueCommand request, CancellationToken cancellationToken)
    {
        var queue = await _queueRepository.GetByTenantIdAndIdAsync(request.TenantId, request.QueueId, cancellationToken);
        if (queue == null)
        {
            throw new InvalidOperationException($"Queue with ID '{request.QueueId}' not found for tenant");
        }

        queue.Deactivate();
        await _queueRepository.UpdateAsync(queue, cancellationToken);
        await _queueRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<QueueDto>(queue);
    }
}
