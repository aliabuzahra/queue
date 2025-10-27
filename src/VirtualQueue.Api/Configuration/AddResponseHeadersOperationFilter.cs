using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VirtualQueue.Api.Configuration;

public class AddResponseHeadersOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add common response headers
        if (operation.Responses.ContainsKey("200"))
        {
            AddCommonHeaders(operation.Responses["200"]);
        }

        if (operation.Responses.ContainsKey("201"))
        {
            AddCommonHeaders(operation.Responses["201"]);
        }

        if (operation.Responses.ContainsKey("400"))
        {
            AddCommonHeaders(operation.Responses["400"]);
        }

        if (operation.Responses.ContainsKey("401"))
        {
            AddCommonHeaders(operation.Responses["401"]);
        }

        if (operation.Responses.ContainsKey("403"))
        {
            AddCommonHeaders(operation.Responses["403"]);
        }

        if (operation.Responses.ContainsKey("404"))
        {
            AddCommonHeaders(operation.Responses["404"]);
        }

        if (operation.Responses.ContainsKey("500"))
        {
            AddCommonHeaders(operation.Responses["500"]);
        }
    }

    private static void AddCommonHeaders(OpenApiResponse response)
    {
        response.Headers ??= new Dictionary<string, OpenApiHeader>();

        response.Headers["X-Request-ID"] = new OpenApiHeader
        {
            Description = "Unique request identifier for tracking",
            Schema = new OpenApiSchema { Type = "string", Format = "uuid" }
        };

        response.Headers["X-Response-Time"] = new OpenApiHeader
        {
            Description = "Response time in milliseconds",
            Schema = new OpenApiSchema { Type = "integer" }
        };

        response.Headers["X-Tenant-ID"] = new OpenApiHeader
        {
            Description = "Tenant identifier for the request",
            Schema = new OpenApiSchema { Type = "string", Format = "uuid" }
        };
    }
}

