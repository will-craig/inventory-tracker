using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Stockpile.Api.Controllers;
using Stockpile.Api.Services;
using Stockpile.Domain.Entities;

namespace Stockpile.UnitTests.Controllers;

[Collection("Controller Tests")]
public class StockControllerTests
{
    [Fact]
    public async Task GetStock_ReturnsOk_WhenUserIsAuthenticated()
    {
        var inventoryServiceMock = new Mock<IInventoryService>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.Username).Returns("DemoUser1");
        inventoryServiceMock.Setup(x => x.GetInventoryItemsByUserAsync("DemoUser1"))
            .ReturnsAsync(new List<InventoryItem> { new InventoryItem
                {
                    Username = "DemoUser1",
                    Name = string.Empty,
                    UserId = string.Empty
                }
            });

        var controller = new StockController(inventoryServiceMock.Object, currentUserServiceMock.Object);
        var result = await controller.GetStock();
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStock_ReturnsUnauthorized_WhenUsernameIsNull()
    {
        var inventoryServiceMock = new Mock<IInventoryService>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.Username).Returns((string?)null); // Set username to null
        var controller = new StockController(inventoryServiceMock.Object, currentUserServiceMock.Object);
        var result = await controller.GetStock();
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetStockById_ReturnsNotFound_WhenItemDoesNotExist()
    {
        var inventoryServiceMock = new Mock<IInventoryService>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        inventoryServiceMock.Setup(x => x.GetInventoryItemAsync("item1")).ReturnsAsync((InventoryItem)null!);
        var controller = new StockController(inventoryServiceMock.Object, currentUserServiceMock.Object);
        var result = await controller.GetStock("item1");
        result.Result.Should().BeOfType<NotFoundObjectResult>(); // Expect NotFoundObjectResult, not NotFoundResult
    }

    [Fact]
    public async Task GetStockById_ReturnsUnauthorized_WhenUserDoesNotOwnItem()
    {
        var inventoryServiceMock = new Mock<IInventoryService>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.Username).Returns("DemoUser1");
        inventoryServiceMock.Setup(x => x.GetInventoryItemAsync("item1"))
            .ReturnsAsync(new InventoryItem
            {
                Username = "OtherUser",
                Name = string.Empty,
                UserId = string.Empty
            });
        var controller = new StockController(inventoryServiceMock.Object, currentUserServiceMock.Object);
        var result = await controller.GetStock("item1");
        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task UpdateStock_ReturnsNotFound_WhenItemDoesNotExist()
    {
        var inventoryServiceMock = new Mock<IInventoryService>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.UserId).Returns("user1");
        currentUserServiceMock.Setup(x => x.Username).Returns("DemoUser1");
        inventoryServiceMock.Setup(x => x.GetInventoryItemAsync("item1")).ReturnsAsync((InventoryItem)null!);

        var controller = new StockController(inventoryServiceMock.Object, currentUserServiceMock.Object);
        var result = await controller.UpdateStock("item1", new()
        {
            Name = "Milk"
        });

        result.Result.Should().BeOfType<NotFoundObjectResult>();
        inventoryServiceMock.Verify(x => x.UpdateInventoryItemAsync(It.IsAny<InventoryItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStock_ReturnsForbidden_WhenUserDoesNotOwnItem()
    {
        var inventoryServiceMock = new Mock<IInventoryService>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.UserId).Returns("user1");
        currentUserServiceMock.Setup(x => x.Username).Returns("DemoUser1");
        inventoryServiceMock.Setup(x => x.GetInventoryItemAsync("item1"))
            .ReturnsAsync(new InventoryItem
            {
                Username = "OtherUser",
                Name = "Milk",
                UserId = "other-user"
            });

        var controller = new StockController(inventoryServiceMock.Object, currentUserServiceMock.Object);
        var result = await controller.UpdateStock("item1", new()
        {
            Name = "Milk"
        });

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        inventoryServiceMock.Verify(x => x.UpdateInventoryItemAsync(It.IsAny<InventoryItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStock_UpdatesItem_WhenUserOwnsItem()
    {
        var inventoryServiceMock = new Mock<IInventoryService>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.UserId).Returns("user1");
        currentUserServiceMock.Setup(x => x.Username).Returns("DemoUser1");
        inventoryServiceMock.Setup(x => x.GetInventoryItemAsync("item1"))
            .ReturnsAsync(new InventoryItem
            {
                Username = "DemoUser1",
                Name = "Old Milk",
                UserId = "user1"
            });

        var controller = new StockController(inventoryServiceMock.Object, currentUserServiceMock.Object);
        var result = await controller.UpdateStock("item1", new()
        {
            Name = "Milk",
            Quantity = 2
        });

        result.Result.Should().BeOfType<OkObjectResult>();
        inventoryServiceMock.Verify(x => x.UpdateInventoryItemAsync(It.Is<InventoryItem>(item =>
            item.Id == "item1" &&
            item.Name == "Milk" &&
            item.Quantity == 2 &&
            item.UserId == "user1" &&
            item.Username == "DemoUser1")), Times.Once);
    }
}
