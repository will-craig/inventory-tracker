using Stockpile.Api.Configuration;
using Stockpile.Api.Services;

var initializeDatabaseOnly = args.Contains("--initialize-database", StringComparer.OrdinalIgnoreCase);

var builder = WebApplication.CreateBuilder(args);

var app = builder
    .RegisterServices()
    .Build()
    .ConfigureMiddleware();

var databaseInitializer = app.Services.GetRequiredService<IDatabaseInitializer>();
if (initializeDatabaseOnly)
{
    await databaseInitializer.InitializeAsync();
    return;
}

await databaseInitializer.InitializeOnStartupAsync();
await app.RunAsync();
