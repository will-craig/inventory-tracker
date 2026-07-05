using FluentAssertions;
using Moq;
using Stockpile.Api.Services;
using Stockpile.DAL.Repositories;
using Stockpile.Domain.Entities;
using Stockpile.Domain.Enums;
using Stockpile.Domain.Models;

namespace Stockpile.UnitTests.Services;

public class InventoryServiceTests
{
    [Fact]
    public async Task QueryInventoryForAgentAsync_ClampsLimitAndPassesFilters()
    {
        var repository = new Mock<IInventoryItemRepository>();
        repository.Setup(repo => repo.QueryForAgentAsync(
                "user1",
                "milk",
                "Dairy",
                "Fridge",
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                true,
                InventoryAgentSort.Name,
                true,
                200))
            .ReturnsAsync([]);

        var service = new InventoryService(repository.Object);
        await service.QueryInventoryForAgentAsync(new InventoryAgentQuery(
            "user1",
            "milk",
            "Dairy",
            "Fridge",
            new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc),
            true,
            InventoryAgentSort.Name,
            true,
            500));

        repository.VerifyAll();
    }

    [Fact]
    public async Task GetInventoryDigestForAgentAsync_GroupsItemsIntoExclusiveWindows()
    {
        var asOf = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var repository = new Mock<IInventoryItemRepository>();
        repository.Setup(repo => repo.GetDigestCandidatesAsync("user1", asOf.AddDays(10).Date.AddDays(1).AddTicks(-1), true))
            .ReturnsAsync([
                NewItem("expired", asOf.AddDays(-1)),
                NewItem("two-days", asOf.AddDays(2)),
                NewItem("five-days", asOf.AddDays(5)),
                NewItem("ten-days", asOf.AddDays(10)),
                NewItem("no-expiry", null)
            ]);

        var service = new InventoryService(repository.Object);
        var digest = await service.GetInventoryDigestForAgentAsync(new InventoryDigestOptions(
            "user1",
            asOf,
            [2, 5, 10],
            25,
            true));

        digest.Expired.Should().ContainSingle(item => item.Name == "expired");
        digest.DueWithinWindows[2].Should().ContainSingle(item => item.Name == "two-days");
        digest.DueWithinWindows[5].Should().ContainSingle(item => item.Name == "five-days");
        digest.DueWithinWindows[10].Should().ContainSingle(item => item.Name == "ten-days");
        digest.NoExpiry.Should().ContainSingle(item => item.Name == "no-expiry");
    }

    private static InventoryItem NewItem(string name, DateTime? expiryDate)
    {
        return new InventoryItem
        {
            Name = name,
            Quantity = 1,
            Unit = Unit.Part,
            ExpiryDate = expiryDate,
            UserId = "user1",
            Username = "AgentUser"
        };
    }
}