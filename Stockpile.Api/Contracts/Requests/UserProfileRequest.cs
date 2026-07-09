using Stockpile.Domain.Entities;

namespace Stockpile.Api.Contracts.Requests;

public record UserProfileRequest
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required UserPreferences Preferences { get; set; }
}