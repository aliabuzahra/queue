using AutoMapper;
using MediatR;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Tenants;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantDto?>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IMapper _mapper;

    public GetTenantByIdQueryHandler(ITenantRepository tenantRepository, IMapper mapper)
    {
        _tenantRepository = tenantRepository;
        _mapper = mapper;
    }

    public async Task<TenantDto?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.Id, cancellationToken);
        return tenant == null ? null : _mapper.Map<TenantDto>(tenant);
    }
}
