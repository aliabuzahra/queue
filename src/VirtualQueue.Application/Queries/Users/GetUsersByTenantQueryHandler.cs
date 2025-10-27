using MediatR;
using Microsoft.Extensions.Logging;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Users;

public class GetUsersByTenantQueryHandler : IRequestHandler<GetUsersByTenantQuery, List<UserDto>>
{
    private readonly IUserService _userService;
    private readonly ILogger<GetUsersByTenantQueryHandler> _logger;

    public GetUsersByTenantQueryHandler(IUserService userService, ILogger<GetUsersByTenantQueryHandler> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<List<UserDto>> Handle(GetUsersByTenantQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting users for tenant {TenantId}", request.TenantId);
        return await _userService.GetUsersByTenantAsync(request.TenantId, cancellationToken);
    }
}
