using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Stockpile.Api.Configuration.Models;
using Stockpile.Api.Services;
using Stockpile.DAL.Repositories;

namespace Stockpile.UnitTests.Services;

public class DatabaseInitializerTests
{
    [Fact]
    public async Task InitializeOnStartupAsync_DoesNothing_WhenRunOnStartupIsDisabled()
    {
        var repository = new Mock<IInventoryItemRepository>();
        var initializer = CreateInitializer(new DatabaseInitializationConfig
        {
            RunOnStartup = false,
            SeedData = false,
            EnsureIndexes = true
        }, repository);

        await initializer.InitializeOnStartupAsync();

        repository.Verify(repo => repo.EnsureAgentIndexesAsync(), Times.Never);
    }

    [Fact]
    public async Task InitializeAsync_EnsuresIndexes_WhenEnabled()
    {
        var repository = new Mock<IInventoryItemRepository>();
        repository.Setup(repo => repo.EnsureAgentIndexesAsync()).Returns(Task.CompletedTask);
        var initializer = CreateInitializer(new DatabaseInitializationConfig
        {
            SeedData = false,
            EnsureIndexes = true
        }, repository);

        await initializer.InitializeAsync();

        repository.Verify(repo => repo.EnsureAgentIndexesAsync(), Times.Once);
    }

    private static DatabaseInitializer CreateInitializer(
        DatabaseInitializationConfig config,
        Mock<IInventoryItemRepository> repository)
    {
        var services = new ServiceCollection();
        services.AddSingleton(repository.Object);
        services.AddSingleton(Mock.Of<IMongoDatabase>());
        var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IServiceScopeFactory>().Should().NotBeNull();
        return new DatabaseInitializer(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(config));
    }
}
