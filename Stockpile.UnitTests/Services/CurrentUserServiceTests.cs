using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Stockpile.Api.Services;
using Stockpile.Domain.Enums;

namespace Stockpile.UnitTests.Services;

public class CurrentUserServiceTests
{
    [Fact]
    public void ReturnsCurrentUserFromClaims()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, "user-id"),
                new Claim(ClaimTypes.Name, "ClaimUser"),
                new Claim(ClaimTypes.Email, "claim@example.com"),
                new Claim(ClaimTypes.Role, UserRole.Admin.ToString())
            ], "TestAuth"))
        };
        var service = new CurrentUserService(new HttpContextAccessor { HttpContext = httpContext });

        service.UserId.Should().Be("user-id");
        service.Username.Should().Be("ClaimUser");
        service.Email.Should().Be("claim@example.com");
        service.Role.Should().Be(UserRole.Admin);
    }
}
