using FluentAssertions;
using Moq;
using Stockpile.Api.Controllers;
using Stockpile.Api.Services;
using Stockpile.Api.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using Stockpile.Domain.Entities;

namespace Stockpile.UnitTests.Controllers;

[Collection("Controller Tests")]
public class AuthControllerTests
{
    [Fact]
    public async Task Login_ReturnsOk_WhenCredentialsAreValid()
    {
        var tokenServiceMock = new Mock<ITokenService>();
        var userProfileServiceMock = new Mock<IUserProfileService>();
        var userProfile = new UserProfile
        {
            Username = "DemoUser1",
            Email = string.Empty
        };
        userProfileServiceMock.Setup(x => x.GetUserProfile("DemoUser1")).ReturnsAsync(userProfile);
        tokenServiceMock.Setup(x => x.GenerateToken(userProfile)).Returns("token123");

        var controller = new AuthController(tokenServiceMock.Object, userProfileServiceMock.Object);
        var request = new LoginRequest { Username = "DemoUser1", Password = "password" };
        var result = await controller.Login(request);
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be("token123");
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenRequestIsNull()
    {
        var controller = new AuthController(Mock.Of<ITokenService>(), Mock.Of<IUserProfileService>());
        var result = await controller.Login(null);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
    {
        var controller = new AuthController(Mock.Of<ITokenService>(), Mock.Of<IUserProfileService>());
        var request = new LoginRequest { Username = "wrong", Password = "wrong" };
        var result = await controller.Login(request);
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}