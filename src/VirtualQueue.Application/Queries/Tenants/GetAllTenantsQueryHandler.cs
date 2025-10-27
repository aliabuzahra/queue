using AutoMapper;
using MediatR;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Queries.Tenants;

public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, IEnumerable<TenantDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IMapper _mapper;

    public GetAllTenantsQueryHandler(ITenantRepository tenantRepository, IMapper mapper)
    {
        _tenantRepository = tenantRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TenantDto>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _tenantRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<TenantDto>>(tenants);
    }
}
