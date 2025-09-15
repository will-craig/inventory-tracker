using System.Security.Claims;
using Stockpile.Domain.Enums;

namespace Stockpile.Api.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    UserRole? Role { get; }
}

public class CurrentUserService(IHttpContextAccessor context) : ICurrentUserService
{
    public UserRole? Role
    {
        get
        {
            var roleClaim = context.HttpContext?.User.Claims.FirstOrDefault(e => e.Type == ClaimTypes.Role);
            return roleClaim?.Value != null ? (UserRole)Enum.Parse(typeof(UserRole), roleClaim.Value) : null;
        }
    }
    public string? Username => context.HttpContext?.User.Identity?.Name;
    public string? UserId => context.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    public string? Email => context.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
}
