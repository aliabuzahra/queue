using AutoMapper;
using MediatR;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Domain.Entities;

namespace VirtualQueue.Application.Commands.UserSessions;

public class EnqueueUserCommandHandler : IRequestHandler<EnqueueUserCommand, UserSessionDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IQueueRepository _queueRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly IQueueNotificationService _notificationService;

    public EnqueueUserCommandHandler(
        ITenantRepository tenantRepository,
        IQueueRepository queueRepository,
        IUserSessionRepository userSessionRepository,
        ICacheService cacheService,
        IMapper mapper,
        IQueueNotificationService notificationService)
    {
        _tenantRepository = tenantRepository;
        _queueRepository = queueRepository;
        _userSessionRepository = userSessionRepository;
        _cacheService = cacheService;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<UserSessionDto> Handle(EnqueueUserCommand request, CancellationToken cancellationToken)
    {
        // Verify tenant exists and is active
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant with ID '{request.TenantId}' not found");
        }

        if (!tenant.IsActive)
        {
            throw new InvalidOperationException("Cannot enqueue user for inactive tenant");
        }

        // Verify queue exists and is active
        var queue = await _queueRepository.GetByTenantIdAndIdAsync(request.TenantId, request.QueueId, cancellationToken);
        if (queue == null)
        {
            throw new InvalidOperationException($"Queue with ID '{request.QueueId}' not found for tenant");
        }

        if (!queue.IsActive)
        {
            throw new InvalidOperationException("Cannot enqueue user to inactive queue");
        }

        // Check if user is already in queue
        var existingSession = await _userSessionRepository.GetByQueueIdAndUserIdentifierAsync(
            request.QueueId, request.UserIdentifier, cancellationToken);
        
        if (existingSession != null && existingSession.Status != Domain.Enums.QueueStatus.Dropped)
        {
            throw new InvalidOperationException("User is already in queue");
        }

        // Enqueue user
        var userSession = queue.EnqueueUser(request.UserIdentifier, request.Metadata, request.Priority);

        await _userSessionRepository.AddAsync(userSession, cancellationToken);
        await _userSessionRepository.SaveChangesAsync(cancellationToken);

        // Update position in cache
        var position = await _userSessionRepository.GetUserPositionInQueueAsync(
            request.QueueId, request.UserIdentifier, cancellationToken);
        await _cacheService.SetUserPositionAsync(request.QueueId, request.UserIdentifier, position, cancellationToken);

        // Send notification
        await _notificationService.NotifyUserEnqueuedAsync(
            request.TenantId, request.QueueId, request.UserIdentifier, position, cancellationToken);

        return _mapper.Map<UserSessionDto>(userSession);
    }
}
