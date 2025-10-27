using AutoMapper;
using MediatR;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Commands.UserSessions;

public class DropUserCommandHandler : IRequestHandler<DropUserCommand, UserSessionDto?>
{
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IMapper _mapper;

    public DropUserCommandHandler(IUserSessionRepository userSessionRepository, IMapper mapper)
    {
        _userSessionRepository = userSessionRepository;
        _mapper = mapper;
    }

    public async Task<UserSessionDto?> Handle(DropUserCommand request, CancellationToken cancellationToken)
    {
        var userSession = await _userSessionRepository.GetByQueueIdAndUserIdentifierAsync(
            request.QueueId, request.UserIdentifier, cancellationToken);
        
        if (userSession == null)
        {
            return null;
        }

        userSession.MarkAsDropped();
        await _userSessionRepository.UpdateAsync(userSession, cancellationToken);
        await _userSessionRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserSessionDto>(userSession);
    }
}
