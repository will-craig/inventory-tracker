using FluentAssertions;
using MongoDB.Driver;
using Stockpile.DAL.Repositories;
using Stockpile.Domain.Entities;
using Stockpile.Domain.Enums;
using MongoDB.Bson;

namespace Stockpile.IntegrationTests.Repositories;

public class InventoryItemRepositoryIntegrationTests : IntegrationTestBase
{
    private readonly InventoryItemRepository _repository;

    public InventoryItemRepositoryIntegrationTests()
    {
        _repository = new InventoryItemRepository(Database);
    }

    [Fact]
    public async Task InsertAndGetById_WorksCorrectly()
    {
        var item = new InventoryItem
        {
            Name = "Test Item",
            Quantity = 5,
            Unit = Unit.Litre,
            ExpiryDate = null,
            Username = "testuser",
            UserId = MongoDB.Bson.ObjectId.GenerateNewId().ToString()
        };
        await _repository.CreateAsync(item);
        TrackCreatedId(item.Id);
        var result = await _repository.GetByIdAsync(item.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Item");
        result.Quantity.Should().Be(5);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullForMissingItem()
    {
        var result = await _repository.GetByIdAsync("000000000000000000000000");
        result.Should().BeNull();
    }

    protected override async Task DeleteEntityAsync(string id)
    {
        await _repository.DeleteAsync(id);
    }
}
