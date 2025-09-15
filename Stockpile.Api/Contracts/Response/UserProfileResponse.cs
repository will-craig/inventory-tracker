using Stockpile.Domain.Entities;

namespace Stockpile.Api.Contracts.Response;

public record UserProfileResponse
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public UserPreferences? Preferences { get; set; }
}