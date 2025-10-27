using Microsoft.AspNetCore.Http;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITenantRepository _tenantRepository;

    public TenantMiddleware(RequestDelegate next, ITenantRepository tenantRepository)
    {
        _next = next;
        _tenantRepository = tenantRepository;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // Try to get tenant from X-Tenant-Key header
        if (context.Request.Headers.TryGetValue("X-Tenant-Key", out var apiKey))
        {
            var tenant = await _tenantRepository.GetByApiKeyAsync(apiKey.ToString());
            if (tenant != null)
            {
                tenantContext.TenantId = tenant.Id;
                tenantContext.TenantDomain = tenant.Domain;
            }
        }
        // Try to get tenant from domain
        else if (context.Request.Host.HasValue)
        {
            var domain = context.Request.Host.Host;
            var tenant = await _tenantRepository.GetByDomainAsync(domain);
            if (tenant != null)
            {
                tenantContext.TenantId = tenant.Id;
                tenantContext.TenantDomain = tenant.Domain;
            }
        }

        await _next(context);
    }
}
