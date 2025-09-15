using Stockpile.Api.Contracts.Requests;
using Stockpile.Api.Contracts.Response;
using Stockpile.Domain.Entities;

namespace Stockpile.Api.Contracts.Mappers;

public static class UserProfileMapper
{
    public static UserProfileResponse MapFrom(this UserProfile userProfile)
    {
        return new UserProfileResponse
        {
            Email = userProfile.Email,
            Username = userProfile.Username,
            Preferences = userProfile.Preferences
        };
    }
    
    public static UserProfile MapTo(this UserProfileRequest userProfileRequest)
    {
        return new UserProfile(userProfileRequest.Id)
        {
            Username = userProfileRequest.Username,
            Email = userProfileRequest.Email,
            Preferences = userProfileRequest.Preferences
        };
    }
}