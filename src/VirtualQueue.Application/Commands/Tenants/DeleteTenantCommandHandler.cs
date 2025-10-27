using MediatR;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Application.Commands.Tenants;

public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, bool>
{
    private readonly ITenantRepository _tenantRepository;

    public DeleteTenantCommandHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<bool> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tenant == null)
        {
            return false;
        }

        await _tenantRepository.DeleteAsync(tenant, cancellationToken);
        await _tenantRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
