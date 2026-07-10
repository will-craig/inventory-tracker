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
        digest.Counts.Should().BeEquivalentTo(new InventoryDigestCounts(
            1,
            new Dictionary<int, int>
            {
                [2] = 1,
                [5] = 1,
                [10] = 1
            },
            1,
            4));
        digest.Hints.Should().Contain(hint => hint.Type == "expired-items");
        digest.Hints.Should().Contain(hint => hint.Type == "use-first");
        digest.Hints.Should().Contain(hint => hint.Type == "check-quantity");
        digest.Hints.Should().Contain(hint => hint.Type == "missing-expiry");
    }

    [Fact]
    public async Task GetInventoryDigestForAgentAsync_BuildsHintsBeforeReturningDigest()
    {
        var asOf = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var expired = NewItem("expired", asOf.AddDays(-1));
        var dueSoon = NewItem("two-days", asOf.AddDays(2));
        dueSoon.Quantity = 0.5f;
        var noExpiry = NewItem("no-expiry", null);
        var repository = new Mock<IInventoryItemRepository>();
        repository.Setup(repo => repo.GetDigestCandidatesAsync("user1", asOf.AddDays(2).Date.AddDays(1).AddTicks(-1), true))
            .ReturnsAsync([expired, dueSoon, noExpiry]);

        var service = new InventoryService(repository.Object);
        var digest = await service.GetInventoryDigestForAgentAsync(new InventoryDigestOptions(
            "user1",
            asOf,
            [2],
            25,
            true));

        digest.Hints.Should().BeEquivalentTo([
            new InventoryAgentHint(
                "expired-items",
                "high",
                "1 item(s) are already expired. The agent should ask the user to inspect before using.",
                [expired.Id]),
            new InventoryAgentHint(
                "use-first",
                "medium",
                "1 item(s) should be prioritized before later inventory.",
                [dueSoon.Id]),
            new InventoryAgentHint(
                "check-quantity",
                "medium",
                "1 urgent item(s) have quantity 1 or less; the agent should avoid assuming much is available.",
                [dueSoon.Id]),
            new InventoryAgentHint(
                "missing-expiry",
                "low",
                "1 item(s) have no expiry date and may need user confirmation.",
                [noExpiry.Id])
        ], options => options.WithStrictOrdering());
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
