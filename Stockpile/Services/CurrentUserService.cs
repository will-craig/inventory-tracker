using System.Security.Claims;
using Stockpile.DAL.Models;
using Stockpile.DAL.Models.Enums;

namespace Stockpile.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    Role? Role { get; }
}

public class CurrentUserService(IHttpContextAccessor context) : ICurrentUserService
{
    public Role? Role
    {
        get
        {
            var roleClaim = context.HttpContext?.User.Claims.FirstOrDefault(e => e.Type == ClaimTypes.Role);
            return roleClaim?.Value != null ? (Role)Enum.Parse(typeof(Role), roleClaim.Value) : null;
        }
    }
    public string? Username => context.HttpContext?.User.Identity?.Name;
    public string? UserId => context.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    public string? Email => context.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
}
