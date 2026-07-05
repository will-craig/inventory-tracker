using MongoDB.Driver;
using Stockpile.Api.Configuration;
using Stockpile.DAL.Repositories;

var app = WebApplication
    .CreateBuilder(args)
    .RegisterServices()
    .Build()
    .ConfigureMiddleware();

await InitializeDatabaseAsync();
await app.RunAsync();

async Task InitializeDatabaseAsync()
{
    var serviceProvider = app.Services;
    using var scope = serviceProvider.CreateScope();
    var scopedProvider = scope.ServiceProvider;
    var db = scopedProvider.GetRequiredService<IMongoDatabase>();
    SeedData.Initialize(db);
    var inventoryRepository = scopedProvider.GetRequiredService<IInventoryItemRepository>();
    await inventoryRepository.EnsureAgentIndexesAsync();
}
