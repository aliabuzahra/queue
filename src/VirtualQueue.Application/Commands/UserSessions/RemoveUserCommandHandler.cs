using MediatR;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Application.Commands.UserSessions;

public class RemoveUserCommandHandler : IRequestHandler<RemoveUserCommand, bool>
{
    private readonly IUserSessionRepository _userSessionRepository;

    public RemoveUserCommandHandler(IUserSessionRepository userSessionRepository)
    {
        _userSessionRepository = userSessionRepository;
    }

    public async Task<bool> Handle(RemoveUserCommand request, CancellationToken cancellationToken)
    {
        var userSession = await _userSessionRepository.GetByQueueIdAndUserIdentifierAsync(
            request.QueueId, request.UserIdentifier, cancellationToken);
        
        if (userSession == null)
        {
            return false;
        }

        await _userSessionRepository.DeleteAsync(userSession, cancellationToken);
        await _userSessionRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
