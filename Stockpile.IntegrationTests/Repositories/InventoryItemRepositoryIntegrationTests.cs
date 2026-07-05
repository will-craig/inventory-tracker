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

    [Fact]
    public async Task QueryForAgentAsync_FiltersByUserExpiryAndMetadata()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var otherUserId = ObjectId.GenerateNewId().ToString();
        var expiringItem = NewItem("Milk", userId, DateTime.UtcNow.AddDays(2), "Dairy", "Fridge");
        var otherUserItem = NewItem("Milk", otherUserId, DateTime.UtcNow.AddDays(2), "Dairy", "Fridge");
        var noExpiryItem = NewItem("Rice", userId, null, "Pantry", "Cupboard");

        await _repository.CreateAsync(expiringItem);
        await _repository.CreateAsync(otherUserItem);
        await _repository.CreateAsync(noExpiryItem);
        TrackCreatedId(expiringItem.Id);
        TrackCreatedId(otherUserItem.Id);
        TrackCreatedId(noExpiryItem.Id);

        var result = await _repository.QueryForAgentAsync(
            userId,
            "milk",
            "dairy",
            "fridge",
            null,
            DateTime.UtcNow.AddDays(5),
            false,
            InventoryAgentSort.ExpiryDate,
            false,
            20);

        result.Should().ContainSingle(item => item.Id == expiringItem.Id);
    }

    [Fact]
    public async Task GetDigestCandidatesAsync_CanIncludeNoExpiryItems()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var expiringItem = NewItem("Cheese", userId, DateTime.UtcNow.AddDays(1), "Dairy", "Fridge");
        var noExpiryItem = NewItem("Salt", userId, null, "Pantry", "Cupboard");

        await _repository.CreateAsync(expiringItem);
        await _repository.CreateAsync(noExpiryItem);
        TrackCreatedId(expiringItem.Id);
        TrackCreatedId(noExpiryItem.Id);

        var result = await _repository.GetDigestCandidatesAsync(userId, DateTime.UtcNow.AddDays(10), true);

        result.Should().Contain(item => item.Id == expiringItem.Id);
        result.Should().Contain(item => item.Id == noExpiryItem.Id);
    }

    private static InventoryItem NewItem(string name, string userId, DateTime? expiryDate, string category, string location)
    {
        return new InventoryItem
        {
            Name = name,
            Quantity = 1,
            Unit = Unit.Part,
            ExpiryDate = expiryDate,
            Category = category,
            Location = location,
            Username = "testuser",
            UserId = userId
        };
    }

    protected override async Task DeleteEntityAsync(string id)
    {
        await _repository.DeleteAsync(id);
    }
}
