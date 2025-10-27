using AutoMapper;
using MediatR;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Domain.Entities;

namespace VirtualQueue.Application.Commands.Queues;

public class CreateQueueCommandHandler : IRequestHandler<CreateQueueCommand, QueueDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IQueueRepository _queueRepository;
    private readonly IMapper _mapper;

    public CreateQueueCommandHandler(
        ITenantRepository tenantRepository,
        IQueueRepository queueRepository,
        IMapper mapper)
    {
        _tenantRepository = tenantRepository;
        _queueRepository = queueRepository;
        _mapper = mapper;
    }

    public async Task<QueueDto> Handle(CreateQueueCommand request, CancellationToken cancellationToken)
    {
        // Verify tenant exists
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant with ID '{request.TenantId}' not found");
        }

        if (!tenant.IsActive)
        {
            throw new InvalidOperationException("Cannot create queue for inactive tenant");
        }

        var queue = new Queue(
            request.TenantId,
            request.Name,
            request.Description,
            request.MaxConcurrentUsers,
            request.ReleaseRatePerMinute);

        await _queueRepository.AddAsync(queue, cancellationToken);
        await _queueRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<QueueDto>(queue);
    }
}
