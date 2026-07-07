using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Stockpile.Api.Configuration.Models;
using Stockpile.Api.Controllers;
using Stockpile.Api.Contracts.Requests;
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

    [Fact]
    public async Task AddInventory_CreatesBulkAdditionsForAgentUser()
    {
        var controller = CreateController(out var inventoryServiceMock);
        inventoryServiceMock.Setup(service => service.AddInventoryForAgentAsync(
                "user1",
                "AgentUser",
                It.Is<IReadOnlyList<InventoryAgentAddition>>(items =>
                    items.Count == 2 &&
                    items[0].Name == "Milk" &&
                    items[1].Reason == "receipt-scan")))
            .ReturnsAsync([
                new InventoryAgentWriteResult("created", "item1", "Milk", null, NewItem("Milk", null)),
                new InventoryAgentWriteResult("created", "item2", "Eggs", null, NewItem("Eggs", null))
            ]);

        var result = await controller.AddInventory(new InventoryAgentBulkAddRequest
        {
            Items =
            [
                new() { Name = "Milk", Quantity = 1, Unit = Unit.Litre },
                new() { Name = "Eggs", Quantity = 12, Unit = Unit.Part, Reason = "receipt-scan" }
            ]
        });

        var response = result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<InventoryAgentBulkWriteResponse>().Subject;
        response.Results.Should().HaveCount(2);
        response.Results.Should().OnlyContain(item => item.Status == "created");
    }

    [Fact]
    public async Task AddInventory_ReturnsUnauthorized_WhenUserContextIsMissing()
    {
        var controller = CreateController(out _, userId: null);

        var result = await controller.AddInventory(new InventoryAgentBulkAddRequest());

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task AddInventory_TreatsNullItemsListAsEmptyBatch()
    {
        var controller = CreateController(out var inventoryServiceMock);
        inventoryServiceMock.Setup(service => service.AddInventoryForAgentAsync(
                "user1",
                "AgentUser",
                It.Is<IReadOnlyList<InventoryAgentAddition>>(items => items.Count == 0)))
            .ReturnsAsync([]);

        var result = await controller.AddInventory(new InventoryAgentBulkAddRequest { Items = null! });

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<InventoryAgentBulkWriteResponse>()
            .Which.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateInventory_AppliesSemanticUpdatesById()
    {
        var controller = CreateController(out var inventoryServiceMock);
        inventoryServiceMock.Setup(service => service.UpdateInventoryForAgentAsync(
                "user1",
                It.Is<IReadOnlyList<InventoryAgentUpdate>>(items =>
                    items.Count == 1 &&
                    items[0].Id == "item1" &&
                    items[0].Quantity == 2 &&
                    items[0].Reason == "inventory-correction")))
            .ReturnsAsync([
                new InventoryAgentWriteResult("updated", "item1", "Milk", null, NewItem("Milk", null))
            ]);

        var result = await controller.UpdateInventory(new InventoryAgentBulkUpdateRequest
        {
            Items =
            [
                new() { Id = "item1", Quantity = 2, Reason = "inventory-correction" }
            ]
        });

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<InventoryAgentBulkWriteResponse>()
            .Which.Results.Should().ContainSingle(item => item.Status == "updated" && item.Id == "item1");
    }

    [Fact]
    public async Task UpdateInventory_MapsNullItemToFailedServiceValidation()
    {
        var controller = CreateController(out var inventoryServiceMock);
        inventoryServiceMock.Setup(service => service.UpdateInventoryForAgentAsync(
                "user1",
                It.Is<IReadOnlyList<InventoryAgentUpdate>>(items => items.Count == 1 && items[0].Id == null)))
            .ReturnsAsync([
                new InventoryAgentWriteResult("failed", null, null, "Item id is required.")
            ]);

        var result = await controller.UpdateInventory(new InventoryAgentBulkUpdateRequest
        {
            Items = [null!]
        });

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<InventoryAgentBulkWriteResponse>()
            .Which.Results.Should().ContainSingle(item => item.Status == "failed");
    }

    [Fact]
    public async Task ConsumeInventoryItem_ReturnsUpdatedItem_WhenPartiallyConsumed()
    {
        var controller = CreateController(out var inventoryServiceMock);
        inventoryServiceMock.Setup(service => service.ConsumeInventoryForAgentAsync(It.Is<InventoryAgentConsumeCommand>(command =>
                command.ItemId == "item1" &&
                command.UserId == "user1" &&
                command.Quantity == 0.5f &&
                command.Reason == "used-in-meal")))
            .ReturnsAsync(new InventoryAgentConsumeResult("updated", null, NewItem("Milk", null)));

        var result = await controller.ConsumeInventoryItem("item1", new ConsumeInventoryItemRequest
        {
            Quantity = 0.5f,
            Reason = "used-in-meal"
        });

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<InventoryAgentItemResponse>()
            .Which.Name.Should().Be("Milk");
    }

    [Fact]
    public async Task ConsumeInventoryItem_ReturnsNoContent_WhenItemIsDepleted()
    {
        var controller = CreateController(out var inventoryServiceMock);
        inventoryServiceMock.Setup(service => service.ConsumeInventoryForAgentAsync(It.IsAny<InventoryAgentConsumeCommand>()))
            .ReturnsAsync(new InventoryAgentConsumeResult("deleted", null));

        var result = await controller.ConsumeInventoryItem("item1", new ConsumeInventoryItemRequest { Quantity = 1 });

        result.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ConsumeInventoryItem_ReturnsForbidden_WhenServiceReportsForeignItem()
    {
        var controller = CreateController(out var inventoryServiceMock);
        inventoryServiceMock.Setup(service => service.ConsumeInventoryForAgentAsync(It.IsAny<InventoryAgentConsumeCommand>()))
            .ReturnsAsync(new InventoryAgentConsumeResult("failed", "You do not have access to this item."));

        var result = await controller.ConsumeInventoryItem("item1", new ConsumeInventoryItemRequest { Quantity = 1 });

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task DeleteInventoryItem_ReturnsNoContent_WhenDeleted()
    {
        var controller = CreateController(out var inventoryServiceMock);
        inventoryServiceMock.Setup(service => service.DeleteInventoryForAgentAsync("item1", "user1"))
            .ReturnsAsync(new InventoryAgentWriteResult("deleted", "item1", "Milk", null));

        var result = await controller.DeleteInventoryItem("item1");

        result.Should().BeOfType<NoContentResult>();
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
