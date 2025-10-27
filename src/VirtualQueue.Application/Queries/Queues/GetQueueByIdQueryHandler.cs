using AutoMapper;
using MediatR;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Queues;

public class GetQueueByIdQueryHandler : IRequestHandler<GetQueueByIdQuery, QueueDto?>
{
    private readonly IQueueRepository _queueRepository;
    private readonly IMapper _mapper;

    public GetQueueByIdQueryHandler(IQueueRepository queueRepository, IMapper mapper)
    {
        _queueRepository = queueRepository;
        _mapper = mapper;
    }

    public async Task<QueueDto?> Handle(GetQueueByIdQuery request, CancellationToken cancellationToken)
    {
        var queue = await _queueRepository.GetByTenantIdAndIdAsync(request.TenantId, request.QueueId, cancellationToken);
        return queue == null ? null : _mapper.Map<QueueDto>(queue);
    }
}
