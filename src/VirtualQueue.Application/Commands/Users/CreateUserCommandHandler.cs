using MediatR;
using Microsoft.Extensions.Logging;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Domain.Entities;
using VirtualQueue.Domain.Enums;

namespace VirtualQueue.Application.Commands.Users;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserService _userService;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IUserService userService,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _userService = userService;
        _logger = logger;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating user {Username} for tenant {TenantId}", request.Username, request.TenantId);

        // Check if username already exists
        if (await _userRepository.ExistsByUsernameAsync(request.TenantId, request.Username, cancellationToken))
        {
            throw new InvalidOperationException($"Username '{request.Username}' already exists");
        }

        // Check if email already exists
        if (await _userRepository.ExistsByEmailAsync(request.TenantId, request.Email, cancellationToken))
        {
            throw new InvalidOperationException($"Email '{request.Email}' already exists");
        }

        // Parse role
        if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
        {
            throw new ArgumentException($"Invalid role: {request.Role}");
        }

        // Create user request
        var createUserRequest = new CreateUserRequest(
            request.Username,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            userRole);

        // Create user through service
        var userDto = await _userService.CreateUserAsync(createUserRequest, cancellationToken);

        _logger.LogInformation("User {Username} created successfully with ID {UserId}", request.Username, userDto.Id);
        return userDto;
    }
}
