using MediatR;
using Microsoft.Extensions.Logging;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Commands.Users;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UserDto>
{
    private readonly IUserService _userService;
    private readonly ILogger<UpdateUserProfileCommandHandler> _logger;

    public UpdateUserProfileCommandHandler(IUserService userService, ILogger<UpdateUserProfileCommandHandler> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<UserDto> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating profile for user {UserId}", request.UserId);

        var updateRequest = new UpdateUserProfileRequest(
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.ProfileImageUrl);

        await _userService.UpdateUserProfileAsync(request.UserId, updateRequest, cancellationToken);

      // Get the updated user
      var userDto = await _userService.GetUserAsync(request.UserId, cancellationToken);
      
      if (userDto == null)
      {
          throw new InvalidOperationException($"User with ID {request.UserId} not found after update");
      }

      _logger.LogInformation("Profile updated successfully for user {UserId}", request.UserId);
      return userDto;
    }
}
