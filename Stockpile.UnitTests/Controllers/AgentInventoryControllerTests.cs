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
        controller.Response.Headers["Accept-Query"].Should().Contain("application/json");
    }

    [Fact]
    public async Task QueryInventoryWithBody_UsesSafeQueryRequestBody()
    {
        var controller = CreateController(out var inventoryServiceMock);
        var expiresFrom = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var expiresTo = new DateTime(2026, 7, 6, 0, 0, 0, DateTimeKind.Utc);
        inventoryServiceMock.Setup(service => service.QueryInventoryForAgentAsync(It.Is<InventoryAgentQuery>(query =>
                query.UserId == "user1" &&
                query.Search == "milk" &&
                query.Category == "Dairy" &&
                query.Location == "Fridge" &&
                query.ExpiresFrom == expiresFrom &&
                query.ExpiresTo == expiresTo &&
                query.IncludeNoExpiry &&
                query.Sort == InventoryAgentSort.Name &&
                query.Descending &&
                query.Limit == 10)))
            .ReturnsAsync([
                NewItem("Milk", expiresTo)
            ]);

        var result = await controller.QueryInventoryWithBody(new()
        {
            Search = "milk",
            Category = "Dairy",
            Location = "Fridge",
            ExpiresFrom = expiresFrom,
            ExpiresTo = expiresTo,
            IncludeNoExpiry = true,
            Sort = InventoryAgentSort.Name,
            Descending = true,
            Limit = 10
        });

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<InventoryAgentQueryResponse>()
            .Which.Items.Should().ContainSingle(item => item.Name == "Milk");
        controller.Response.Headers["Accept-Query"].Should().Contain("application/json");
    }

    [Fact]
    public async Task CreateItem_AddsItemForAgentUser()
    {
        var controller = CreateController(out var inventoryServiceMock);
        InventoryItem? capturedItem = null;
        inventoryServiceMock.Setup(service => service.AddInventoryItemAsync(It.IsAny<InventoryItem>()))
            .Callback<InventoryItem>(item => capturedItem = item)
            .Returns(Task.CompletedTask);

        var result = await controller.CreateItem(new InventoryAgentCreateItemRequest
        {
            Name = "Greek yogurt",
            Quantity = 2,
            Unit = Unit.Part,
            ExpiryDate = new DateTime(2026, 7, 12, 0, 0, 0, DateTimeKind.Utc),
            Category = "Dairy",
            Location = "Fridge"
        });

        result.Result.Should().BeOfType<CreatedResult>()
            .Which.Value.Should().BeOfType<InventoryAgentItemResponse>()
            .Which.Name.Should().Be("Greek yogurt");
        capturedItem.Should().NotBeNull();
        capturedItem!.UserId.Should().Be("user1");
        capturedItem.Username.Should().Be("AgentUser");
    }

    [Fact]
    public async Task CreateItem_ReturnsUnauthorized_WhenUsernameIsMissing()
    {
        var controller = CreateController(out _, username: null);

        var result = await controller.CreateItem(new InventoryAgentCreateItemRequest
        {
            Name = "Milk",
            Unit = Unit.Part
        });

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task UpdateItem_AppliesProvidedFieldsOnly()
    {
        var controller = CreateController(out var inventoryServiceMock);
        var item = NewItem("Milk", new DateTime(2026, 7, 12, 0, 0, 0, DateTimeKind.Utc));
        item.Category = "Dairy";
        item.Location = "Fridge";
        inventoryServiceMock.Setup(service => service.GetInventoryItemAsync(item.Id)).ReturnsAsync(item);
        inventoryServiceMock.Setup(service => service.UpdateInventoryItemAsync(item)).Returns(Task.CompletedTask);

        var result = await controller.UpdateItem(item.Id, new InventoryAgentUpdateItemRequest
        {
            Quantity = 0.5f,
            Location = "Door shelf"
        });

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<InventoryAgentItemResponse>()
            .Which.Location.Should().Be("Door shelf");
        item.Name.Should().Be("Milk");
        item.Quantity.Should().Be(0.5f);
        item.Category.Should().Be("Dairy");
    }

    [Fact]
    public async Task UpdateItem_ClearsOptionalFields()
    {
        var controller = CreateController(out var inventoryServiceMock);
        var item = NewItem("Milk", new DateTime(2026, 7, 12, 0, 0, 0, DateTimeKind.Utc));
        item.Category = "Dairy";
        item.Notes = "Opened";
        inventoryServiceMock.Setup(service => service.GetInventoryItemAsync(item.Id)).ReturnsAsync(item);
        inventoryServiceMock.Setup(service => service.UpdateInventoryItemAsync(item)).Returns(Task.CompletedTask);

        var result = await controller.UpdateItem(item.Id, new InventoryAgentUpdateItemRequest
        {
            Clear = ["category", "notes", "expiryDate"]
        });

        result.Result.Should().BeOfType<OkObjectResult>();
        item.Category.Should().BeNull();
        item.Notes.Should().BeNull();
        item.ExpiryDate.Should().BeNull();
    }

    [Fact]
    public async Task UpdateItem_ReturnsBadRequest_WhenNoChangesAreProvided()
    {
        var controller = CreateController(out var inventoryServiceMock);
        var item = NewItem("Milk", null);
        inventoryServiceMock.Setup(service => service.GetInventoryItemAsync(item.Id)).ReturnsAsync(item);

        var result = await controller.UpdateItem(item.Id, new InventoryAgentUpdateItemRequest());

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateItem_ReturnsBadRequest_WhenClearIsNullAndNoChangesAreProvided()
    {
        var controller = CreateController(out var inventoryServiceMock);
        var item = NewItem("Milk", null);
        inventoryServiceMock.Setup(service => service.GetInventoryItemAsync(item.Id)).ReturnsAsync(item);

        var result = await controller.UpdateItem(item.Id, new InventoryAgentUpdateItemRequest { Clear = null });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateItem_ReturnsForbidden_WhenItemBelongsToAnotherUser()
    {
        var controller = CreateController(out var inventoryServiceMock);
        var item = NewItem("Milk", null) with { UserId = "other-user" };
        inventoryServiceMock.Setup(service => service.GetInventoryItemAsync(item.Id)).ReturnsAsync(item);

        var result = await controller.UpdateItem(item.Id, new InventoryAgentUpdateItemRequest { Quantity = 1 });

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task UpdateItem_ReturnsNotFound_WhenItemDoesNotExist()
    {
        var controller = CreateController(out var inventoryServiceMock);
        inventoryServiceMock.Setup(service => service.GetInventoryItemAsync("missing")).ReturnsAsync((InventoryItem?)null);

        var result = await controller.UpdateItem("missing", new InventoryAgentUpdateItemRequest { Quantity = 1 });

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ConsumeItem_DecrementsQuantityAndAppendsNotes()
    {
        var controller = CreateController(out var inventoryServiceMock);
        var item = NewItem("Milk", null);
        item.Quantity = 2;
        item.Notes = "Opened";
        inventoryServiceMock.Setup(service => service.GetInventoryItemAsync(item.Id)).ReturnsAsync(item);
        inventoryServiceMock.Setup(service => service.UpdateInventoryItemAsync(item)).Returns(Task.CompletedTask);

        var result = await controller.ConsumeItem(item.Id, new ConsumeInventoryItemRequest
        {
            Quantity = 0.75f,
            Notes = "Used for pasta"
        });

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<InventoryAgentItemResponse>()
            .Which.Quantity.Should().Be(1.25f);
        item.Notes.Should().Be("Opened\nUsed for pasta");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ConsumeItem_ReturnsBadRequest_WhenQuantityIsNotPositive(float quantity)
    {
        var controller = CreateController(out var inventoryServiceMock);
        var item = NewItem("Milk", null);
        inventoryServiceMock.Setup(service => service.GetInventoryItemAsync(item.Id)).ReturnsAsync(item);

        var result = await controller.ConsumeItem(item.Id, new ConsumeInventoryItemRequest { Quantity = quantity });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ConsumeItem_ReturnsBadRequest_WhenQuantityExceedsAvailable()
    {
        var controller = CreateController(out var inventoryServiceMock);
        var item = NewItem("Milk", null);
        item.Quantity = 1;
        inventoryServiceMock.Setup(service => service.GetInventoryItemAsync(item.Id)).ReturnsAsync(item);

        var result = await controller.ConsumeItem(item.Id, new ConsumeInventoryItemRequest { Quantity = 2 });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void QueryInventoryWithBody_IsRegisteredForQueryVerbAndHiddenFromOpenApi()
    {
        var method = typeof(AgentInventoryController).GetMethod(nameof(AgentInventoryController.QueryInventoryWithBody));

        method.Should().NotBeNull();
        var acceptVerbs = method!.GetCustomAttributes(typeof(AcceptVerbsAttribute), inherit: false)
            .OfType<AcceptVerbsAttribute>()
            .Single();
        acceptVerbs.HttpMethods.Should().Equal("QUERY");
        acceptVerbs.Route.Should().Be("query");

        method.GetCustomAttributes(typeof(ApiExplorerSettingsAttribute), inherit: false)
            .OfType<ApiExplorerSettingsAttribute>()
            .Single()
            .IgnoreApi.Should().BeTrue();
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
                [])
            {
                Counts = new InventoryDigestCounts(
                    1,
                    new Dictionary<int, int>
                    {
                        [2] = 1,
                        [5] = 0,
                        [10] = 0
                    },
                    0,
                    2),
                Hints =
                [
                    new InventoryAgentHint(
                        "expired-items",
                        "high",
                        "1 item(s) are already expired. The agent should ask the user to inspect before using.",
                        ["expired-id"])
                ]
            });

        var result = await controller.GetDigest(new() { AsOf = asOf, LimitPerSection = 5 });

        var digest = result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<InventoryDigestResponse>().Subject;
        digest.Counts.Expired.Should().Be(1);
        digest.Counts.DueWithinDays["2"].Should().Be(1);
        digest.Hints.Should().Contain(hint => hint.Type == "expired-items");
    }

    private static AgentInventoryController CreateController(
        out Mock<IInventoryService> inventoryServiceMock,
        string? userId = "user1",
        string? username = "AgentUser")
    {
        inventoryServiceMock = new Mock<IInventoryService>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(service => service.UserId).Returns(userId);
        currentUserServiceMock.Setup(service => service.Username).Returns(username);

        var controller = new AgentInventoryController(
            inventoryServiceMock.Object,
            currentUserServiceMock.Object,
            Options.Create(new InventoryDigestConfig
            {
                DefaultExpiryWindowsDays = [2, 5, 10]
            }));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
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
