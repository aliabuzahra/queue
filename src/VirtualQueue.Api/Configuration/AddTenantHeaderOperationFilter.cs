using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VirtualQueue.Api.Configuration;

public class AddTenantHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add tenant header parameter for multi-tenant endpoints
        if (context.ApiDescription.RelativePath?.Contains("tenants/{tenantId}") == true)
        {
            operation.Parameters ??= new List<OpenApiParameter>();
            
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Tenant-Key",
                In = ParameterLocation.Header,
                Description = "Tenant identification key",
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Example = new Microsoft.OpenApi.Any.OpenApiString("tenant-key-123")
                }
            });
        }

        // Add rate limiting headers
        operation.Responses.Add("429", new OpenApiResponse
        {
            Description = "Too Many Requests - Rate limit exceeded",
            Headers = new Dictionary<string, OpenApiHeader>
            {
                ["X-RateLimit-Limit"] = new OpenApiHeader
                {
                    Description = "Rate limit per window",
                    Schema = new OpenApiSchema { Type = "integer" }
                },
                ["X-RateLimit-Remaining"] = new OpenApiHeader
                {
                    Description = "Remaining requests in current window",
                    Schema = new OpenApiSchema { Type = "integer" }
                },
                ["X-RateLimit-Reset"] = new OpenApiHeader
                {
                    Description = "Time when rate limit resets",
                    Schema = new OpenApiSchema { Type = "string", Format = "date-time" }
                },
                ["Retry-After"] = new OpenApiHeader
                {
                    Description = "Seconds to wait before retrying",
                    Schema = new OpenApiSchema { Type = "integer" }
                }
            }
        });
    }
}
