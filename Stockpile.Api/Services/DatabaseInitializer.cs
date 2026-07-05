using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Stockpile.Api.Configuration;
using Stockpile.Api.Configuration.Models;
using Stockpile.DAL.Repositories;

namespace Stockpile.Api.Services;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task InitializeOnStartupAsync(CancellationToken cancellationToken = default);
}

public class DatabaseInitializer(
    IServiceScopeFactory scopeFactory,
    IOptions<DatabaseInitializationConfig> options) : IDatabaseInitializer
{
    private DatabaseInitializationConfig Config => options.Value;

    public Task InitializeOnStartupAsync(CancellationToken cancellationToken = default)
    {
        return Config.RunOnStartup
            ? InitializeAsync(cancellationToken)
            : Task.CompletedTask;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var services = scope.ServiceProvider;

        if (Config.SeedData)
        {
            var database = services.GetRequiredService<IMongoDatabase>();
            SeedData.Initialize(database);
        }

        if (Config.EnsureIndexes)
        {
            var inventoryRepository = services.GetRequiredService<IInventoryItemRepository>();
            await inventoryRepository.EnsureAgentIndexesAsync();
        }
    }
}
