using AutoMapper;
using MediatR;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Commands.Tenants;

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, TenantDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IMapper _mapper;

    public UpdateTenantCommandHandler(ITenantRepository tenantRepository, IMapper mapper)
    {
        _tenantRepository = tenantRepository;
        _mapper = mapper;
    }

    public async Task<TenantDto> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant with ID '{request.Id}' not found");
        }

        tenant.UpdateName(request.Name);
        tenant.UpdateDomain(request.Domain);

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _tenantRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TenantDto>(tenant);
    }
}
