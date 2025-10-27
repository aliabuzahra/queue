using AutoMapper;
using MediatR;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Queues;

public class GetQueuesByTenantIdQueryHandler : IRequestHandler<GetQueuesByTenantIdQuery, IEnumerable<QueueDto>>
{
    private readonly IQueueRepository _queueRepository;
    private readonly IMapper _mapper;

    public GetQueuesByTenantIdQueryHandler(IQueueRepository queueRepository, IMapper mapper)
    {
        _queueRepository = queueRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<QueueDto>> Handle(GetQueuesByTenantIdQuery request, CancellationToken cancellationToken)
    {
        var queues = await _queueRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        return _mapper.Map<IEnumerable<QueueDto>>(queues);
    }
}
