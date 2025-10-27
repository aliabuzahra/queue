using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Infrastructure.Data;
using VirtualQueue.Infrastructure.Repositories;
using VirtualQueue.Infrastructure.Services;

namespace VirtualQueue.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<VirtualQueueDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        // Email settings
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        // Repositories
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IQueueRepository, QueueRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Services
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<INotificationService, EmailNotificationService>();
        services.AddScoped<IQueueNotificationService, QueueNotificationService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IUserService, UserService>();
        
        // High Priority Services
        services.AddScoped<IAuthenticationService, JwtAuthenticationService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IRateLimitingService, RedisRateLimitingService>();
        services.AddScoped<IAuditLoggingService, AuditLoggingService>();
        services.AddScoped<IBackupService, BackupService>();
        
        // Medium Priority Services
        services.AddScoped<ITemplateManagementService, TemplateManagementService>();
        services.AddScoped<IWebhookService, WebhookService>();
        services.AddScoped<IAlertManagementService, AlertManagementService>();
        services.AddScoped<IDataRetentionService, DataRetentionService>();

        // Low Priority Services
        services.AddScoped<IQueueLoadBalancingService, QueueLoadBalancingService>();
        services.AddScoped<IBusinessRulesEngine, BusinessRulesEngine>();
        services.AddScoped<IPerformanceProfilingService, PerformanceProfilingService>();
        services.AddScoped<IThirdPartyIntegrationService, ThirdPartyIntegrationService>();

        return services;
    }
}
