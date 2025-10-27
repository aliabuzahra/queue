using AutoMapper;
using MediatR;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.UserSessions;

public class GetUserSessionsByQueueIdQueryHandler : IRequestHandler<GetUserSessionsByQueueIdQuery, IEnumerable<UserSessionDto>>
{
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IMapper _mapper;

    public GetUserSessionsByQueueIdQueryHandler(IUserSessionRepository userSessionRepository, IMapper mapper)
    {
        _userSessionRepository = userSessionRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserSessionDto>> Handle(GetUserSessionsByQueueIdQuery request, CancellationToken cancellationToken)
    {
        var userSessions = await _userSessionRepository.GetByQueueIdAsync(request.QueueId, cancellationToken);
        return _mapper.Map<IEnumerable<UserSessionDto>>(userSessions);
    }
}
