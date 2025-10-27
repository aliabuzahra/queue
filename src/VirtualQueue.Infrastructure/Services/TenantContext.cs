using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; set; }
    public string? TenantDomain { get; set; }
    public bool IsValid => TenantId.HasValue && !string.IsNullOrEmpty(TenantDomain);
}
