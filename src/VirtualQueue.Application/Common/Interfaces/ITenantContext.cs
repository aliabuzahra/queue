namespace VirtualQueue.Application.Common.Interfaces;

public interface ITenantContext
{
    Guid? TenantId { get; set; }
    string? TenantDomain { get; set; }
    bool IsValid { get; }
}
