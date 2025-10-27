using MediatR;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Application.Commands.Queues;

public class DeleteQueueCommandHandler : IRequestHandler<DeleteQueueCommand, bool>
{
    private readonly IQueueRepository _queueRepository;

    public DeleteQueueCommandHandler(IQueueRepository queueRepository)
    {
        _queueRepository = queueRepository;
    }

    public async Task<bool> Handle(DeleteQueueCommand request, CancellationToken cancellationToken)
    {
        var queue = await _queueRepository.GetByTenantIdAndIdAsync(request.TenantId, request.QueueId, cancellationToken);
        if (queue == null)
        {
            return false;
        }

        await _queueRepository.DeleteAsync(queue, cancellationToken);
        await _queueRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
