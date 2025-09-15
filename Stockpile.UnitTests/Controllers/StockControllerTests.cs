using FluentAssertions;
using Moq;
using Stockpile.Api.Controllers;
using Stockpile.Api.Services;
using Stockpile.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

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
        currentUserServiceMock.Setup(x => x.Username).Returns((string)null); // Set username to null
        var controller = new StockController(inventoryServiceMock.Object, currentUserServiceMock.Object);
        var result = await controller.GetStock();
        result.Result.Should().BeOfType<UnauthorizedResult>();
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
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}
