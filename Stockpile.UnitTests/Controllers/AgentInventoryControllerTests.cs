using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Stockpile.Api.Configuration.Models;
using Stockpile.Api.Controllers;
using Stockpile.Api.Contracts.Response;
using Stockpile.Api.Services;
using Stockpile.Domain.Entities;
using Stockpile.Domain.Enums;
using Stockpile.Domain.Models;

namespace Stockpile.UnitTests.Controllers;

[Collection("Controller Tests")]
public class AgentInventoryControllerTests
{
    [Fact]
    public async Task QueryInventory_ReturnsUnauthorized_WhenUserIdIsMissing()
    {
        var controller = CreateController(out _, userId: null);

        var result = await controller.QueryInventory(new());

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task QueryInventory_ReturnsCompactAgentItems()
    {
        var controller = CreateController(out var inventoryServiceMock);
        inventoryServiceMock.Setup(service => service.QueryInventoryForAgentAsync(It.Is<InventoryAgentQuery>(query =>
                query.UserId == "user1" &&
                query.Search == "milk" &&
                query.Limit == 10)))
            .ReturnsAsync([
                NewItem("Milk", new DateTime(2026, 7, 2, 0, 0, 0, DateTimeKind.Utc))
            ]);

        var result = await controller.QueryInventory(new() { Search = "milk", Limit = 10 });

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<InventoryAgentQueryResponse>()
            .Which.Items.Should().ContainSingle(item => item.Name == "Milk");
    }

    [Fact]
    public async Task GetDigest_UsesConfiguredWindowsAndReturnsHints()
    {
        var controller = CreateController(out var inventoryServiceMock);
        var asOf = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        inventoryServiceMock.Setup(service => service.GetInventoryDigestForAgentAsync(It.Is<InventoryDigestOptions>(options =>
                options.UserId == "user1" &&
                options.WindowsDays.SequenceEqual(new[] { 2, 5, 10 }) &&
                options.LimitPerSection == 5)))
            .ReturnsAsync(new InventoryAgentDigest(
                asOf,
                [2, 5, 10],
                [NewItem("Expired milk", asOf.AddDays(-1))],
                new Dictionary<int, IReadOnlyList<InventoryItem>>
                {
                    [2] = [NewItem("Eggs", asOf.AddDays(2))],
                    [5] = [],
                    [10] = []
                },
                []));

        var result = await controller.GetDigest(new() { AsOf = asOf, LimitPerSection = 5 });

        var digest = result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<InventoryDigestResponse>().Subject;
        digest.Counts.Expired.Should().Be(1);
        digest.Counts.DueWithinDays["2"].Should().Be(1);
        digest.Hints.Should().Contain(hint => hint.Type == "expired-items");
    }

    private static AgentInventoryController CreateController(
        out Mock<IInventoryService> inventoryServiceMock,
        string? userId = "user1")
    {
        inventoryServiceMock = new Mock<IInventoryService>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(service => service.UserId).Returns(userId);
        currentUserServiceMock.Setup(service => service.Username).Returns("AgentUser");

        return new AgentInventoryController(
            inventoryServiceMock.Object,
            currentUserServiceMock.Object,
            Options.Create(new InventoryDigestConfig
            {
                DefaultExpiryWindowsDays = [2, 5, 10]
            }));
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
