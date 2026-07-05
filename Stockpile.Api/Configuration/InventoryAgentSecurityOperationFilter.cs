using Microsoft.OpenApi;
using Stockpile.Api.Configuration.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Stockpile.Api.Configuration;

public class InventoryAgentSecurityOperationFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var requirement = new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference(InventoryAgentConfig.AuthenticationScheme, swaggerDoc, null),
                []
            }
        };

        foreach (var path in swaggerDoc.Paths.Where(path => path.Key.StartsWith("/api/agent/", StringComparison.OrdinalIgnoreCase)))
        {
            if (path.Value?.Operations == null)
                continue;

            foreach (var operation in path.Value.Operations.Values)
            {
                operation.Security ??= [];
                operation.Security.Clear();
                operation.Security.Add(requirement);
            }
        }
    }
}