using Stockpile.DAL.Models;

namespace Stockpile.Contracts.Requests;

public class UserProfileRequest
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public UserPreferences Preferences { get; set; }
}