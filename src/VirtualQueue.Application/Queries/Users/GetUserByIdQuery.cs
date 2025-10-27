using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Users;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;
