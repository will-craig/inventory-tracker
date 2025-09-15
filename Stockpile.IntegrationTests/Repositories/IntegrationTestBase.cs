using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Stockpile.IntegrationTests.Repositories;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected IMongoDatabase Database { get; private set; }
    private MongoClient _client;
    protected string DatabaseName;
    protected readonly List<string> CreatedIds = new();

    protected IntegrationTestBase()
    {
        var (connectionString, databaseName) = GetDatabaseConfig();
        _client = new MongoClient(connectionString);
        Database = _client.GetDatabase(databaseName);
        DatabaseName = databaseName;
    }

    protected static (string ConnectionString, string DatabaseName) GetDatabaseConfig()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
        var section = config.GetSection("DatabaseConfig");
        return (section["ConnectionString"], section["DatabaseName"]);
    }

    protected void TrackCreatedId(string id) => CreatedIds.Add(id);

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual async Task DisposeAsync()
    {
        foreach (var id in CreatedIds)
        {
            await DeleteEntityAsync(id);
        }
        CreatedIds.Clear();
    }

    protected abstract Task DeleteEntityAsync(string id);
}
