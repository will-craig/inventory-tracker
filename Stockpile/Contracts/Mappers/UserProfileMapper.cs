using Stockpile.Contracts.Requests;
using Stockpile.Contracts.Response;
using Stockpile.DAL.Models;

namespace Stockpile.Contracts.Mappers;

public static class UserProfileMapper
{
    public static UserProfileResponse Map(this UserProfile userProfile)
    {
        return new UserProfileResponse
        {
            Email = userProfile.Email,
            Username = userProfile.Username,
            Preferences = userProfile.Preferences
        };
    }
    
    public static UserProfile Map(this UserProfileRequest userProfileRequest)
    {
        return new UserProfile
        {
            Id = userProfileRequest.Id,
            Username = userProfileRequest.Username,
            Email = userProfileRequest.Email,
            Preferences = userProfileRequest.Preferences
        };
    }
}