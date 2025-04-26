using Stockpile.DAL.Models;

namespace Stockpile.Contracts.Response;

public class UserProfileResponse
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public UserPreferences? Preferences { get; set; }
}