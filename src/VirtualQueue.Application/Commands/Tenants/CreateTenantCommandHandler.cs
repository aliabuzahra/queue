using AutoMapper;
using MediatR;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Domain.Entities;

namespace VirtualQueue.Application.Commands.Tenants;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, TenantDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IMapper _mapper;

    public CreateTenantCommandHandler(ITenantRepository tenantRepository, IMapper mapper)
    {
        _tenantRepository = tenantRepository;
        _mapper = mapper;
    }

    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // Check if tenant with same domain already exists
        var existingTenant = await _tenantRepository.GetByDomainAsync(request.Domain, cancellationToken);
        if (existingTenant != null)
        {
            throw new InvalidOperationException($"Tenant with domain '{request.Domain}' already exists");
        }

        var tenant = new Tenant(request.Name, request.Domain);
        await _tenantRepository.AddAsync(tenant, cancellationToken);
        await _tenantRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TenantDto>(tenant);
    }
}
