using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Users;

public record GetUsersByTenantQuery(Guid TenantId) : IRequest<List<UserDto>>;
