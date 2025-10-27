using Microsoft.OpenApi.Models;
using System.Reflection;

namespace VirtualQueue.Api.Configuration;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Virtual Queue Management System API",
                Version = "v1",
                Description = "A comprehensive enterprise-grade virtual queue management system built with .NET 8, DDD, CQRS, and Clean Architecture.",
                Contact = new OpenApiContact
                {
                    Name = "Virtual Queue Support",
                    Email = "support@virtualqueue.com",
                    Url = new Uri("https://virtualqueue.com/support")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Add XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Add JWT Authentication
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "API Key authentication using X-API-Key header. Example: \"X-API-Key: {api-key}\"",
                Name = "X-API-Key",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKey"
            });

            c.AddSecurityDefinition("TenantKey", new OpenApiSecurityScheme
            {
                Description = "Tenant identification using X-Tenant-Key header. Example: \"X-Tenant-Key: {tenant-key}\"",
                Name = "X-Tenant-Key",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "TenantKey"
            });

            // Add security requirements
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    Array.Empty<string>()
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "TenantKey"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Add custom operation filters
            c.OperationFilter<AddTenantHeaderOperationFilter>();
            c.OperationFilter<AddResponseHeadersOperationFilter>();
        });

        return services;
    }
}

