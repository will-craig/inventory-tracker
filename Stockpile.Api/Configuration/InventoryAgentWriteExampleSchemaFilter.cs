using System.Text.Json.Nodes;
using Stockpile.Api.Contracts.Requests;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Stockpile.Api.Configuration;

public class InventoryAgentWriteExampleSchemaFilter : ISchemaFilter
{
    public void Apply(Microsoft.OpenApi.IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not Microsoft.OpenApi.OpenApiSchema openApiSchema)
            return;

        if (context.Type == typeof(InventoryAgentCreateItemRequest))
        {
            openApiSchema.Example = new JsonObject
            {
                ["name"] = "Greek yogurt",
                ["quantity"] = 2,
                ["unit"] = "Part",
                ["expiryDate"] = "2026-07-12T00:00:00Z",
                ["category"] = "Dairy",
                ["location"] = "Fridge",
                ["notes"] = "Added from grocery receipt"
            };
            SetPropertyExamples(openApiSchema, new Dictionary<string, JsonNode?>
            {
                ["name"] = "Greek yogurt",
                ["quantity"] = 2,
                ["unit"] = "Part",
                ["expiryDate"] = "2026-07-12T00:00:00Z",
                ["category"] = "Dairy",
                ["location"] = "Fridge",
                ["purchasedDate"] = "2026-07-10T00:00:00Z",
                ["openedDate"] = "2026-07-10T00:00:00Z",
                ["notes"] = "Added from grocery receipt"
            });
        }

        if (context.Type == typeof(InventoryAgentUpdateItemRequest))
        {
            openApiSchema.Example = new JsonObject
            {
                ["quantity"] = 1,
                ["location"] = "Freezer",
                ["clear"] = new JsonArray("openedDate", "notes")
            };
            SetPropertyExamples(openApiSchema, new Dictionary<string, JsonNode?>
            {
                ["name"] = "Greek yogurt",
                ["quantity"] = 1,
                ["unit"] = "Part",
                ["expiryDate"] = "2026-07-12T00:00:00Z",
                ["category"] = "Dairy",
                ["location"] = "Freezer",
                ["purchasedDate"] = "2026-07-10T00:00:00Z",
                ["openedDate"] = "2026-07-10T00:00:00Z",
                ["notes"] = "Move to freezer",
                ["clear"] = new JsonArray("openedDate", "notes")
            });
        }

        if (context.Type == typeof(ConsumeInventoryItemRequest))
        {
            openApiSchema.Example = new JsonObject
            {
                ["quantity"] = 0.5,
                ["notes"] = "Used for dinner"
            };
            SetPropertyExamples(openApiSchema, new Dictionary<string, JsonNode?>
            {
                ["quantity"] = 0.5,
                ["notes"] = "Used for dinner"
            });
        }
    }

    private static void SetPropertyExamples(
        Microsoft.OpenApi.OpenApiSchema schema,
        IReadOnlyDictionary<string, JsonNode?> examples)
    {
        foreach (var (propertyName, example) in examples)
        {
            if (schema.Properties?.TryGetValue(propertyName, out var propertySchema) == true &&
                propertySchema is Microsoft.OpenApi.OpenApiSchema openApiPropertySchema)
            {
                openApiPropertySchema.Example = example;
            }
        }
    }
}
