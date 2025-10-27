using MediatR;
using Microsoft.Extensions.Logging;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Users;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserService _userService;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(IUserService userService, ILogger<GetUserByIdQueryHandler> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting user {UserId}", request.UserId);
        return await _userService.GetUserAsync(request.UserId, cancellationToken);
    }
}
