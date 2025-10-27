using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Commands.Users;

public record UpdateUserProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? ProfileImageUrl) : IRequest<UserDto>;
