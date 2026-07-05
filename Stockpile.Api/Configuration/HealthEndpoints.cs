using MongoDB.Bson;
using MongoDB.Driver;

namespace Stockpile.Api.Configuration;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapStockpileHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/healthz", async (
                IMongoDatabase database,
                IHostEnvironment environment,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    await database.RunCommandAsync<BsonDocument>(
                        new BsonDocument("ping", 1),
                        cancellationToken: cancellationToken);

                    return Results.Ok(new
                    {
                        status = "healthy",
                        database = "reachable"
                    });
                }
                catch (Exception exception)
                {
                    return Results.Problem(
                        title: "MongoDB is unavailable.",
                        detail: environment.IsDevelopment() || environment.IsEnvironment("AgentLocal")
                            ? exception.Message
                            : null,
                        statusCode: StatusCodes.Status503ServiceUnavailable);
                }
            })
            .AllowAnonymous();

        return endpoints;
    }
}
