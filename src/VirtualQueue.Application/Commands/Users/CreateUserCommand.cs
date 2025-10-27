using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Commands.Users;

public record CreateUserCommand(
    Guid TenantId,
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Role) : IRequest<UserDto>;
