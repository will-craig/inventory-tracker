using Stockpile.DAL.Models;
using Stockpile.DAL.Repositories;
using Stockpile.Exceptions;

namespace Stockpile.Services;

public interface IUserProfileService
{
    Task UpdateUsernameAsync(UserProfile updatedUserProfile);
    Task<UserProfile> GetUserProfile(string username);
    Task<UserProfile> CreateUserProfile(UserProfile userProfile);
}

public class UserProfileService(IUserProfileRepository userRepo, IInventoryItemRepository inventoryRepo) : IUserProfileService
{
    public async Task UpdateUsernameAsync(UserProfile updatedUserProfile)
    {
        // Find UserProfile
        var userProfile = await userRepo.GetByIdAsync(updatedUserProfile.Id);
        
        if (userProfile == null)
            throw new UserNotFoundException($"User with ID {updatedUserProfile.Id} not found");

        await ValidateUserProfile(userProfile);

        // Update UserProfile 
        await userRepo.UpdateAsync(userProfile.Id, updatedUserProfile);
        
        // Update all InventoryItems linked to the user if username changed
        if(!String.Equals(updatedUserProfile.Username, userProfile.Username, StringComparison.CurrentCultureIgnoreCase))
            await inventoryRepo.UpdateUsernameOnCollection(updatedUserProfile.Id, updatedUserProfile.Username);
    }

    public async Task<UserProfile> GetUserProfile(string username)
    {
        var result = await userRepo.GetByUsernameAsync(username);
        
        if (result == null)
            throw new UserNotFoundException($"User with name {username} not found");
        
        return result;
    }

    public async Task<UserProfile> CreateUserProfile(UserProfile userProfile)
    {
        await ValidateUserProfile(userProfile);
        await userRepo.CreateAsync(userProfile);
        return userProfile;
    }
    
    private async Task ValidateUserProfile(UserProfile userProfile)
    {
        //Check if the email is valid
        var existingEmail = await userRepo.FindAsync(e
            => String.Equals(e.Email, userProfile.Email, StringComparison.CurrentCultureIgnoreCase));
        
        if (existingEmail != null && existingEmail.Count != 0)
            throw new EmailAlreadyTakenException();

        //Check if the new username is already taken
        var existingUser = await userRepo.FindAsync(e
            => String.Equals(e.Username, userProfile.Username, StringComparison.CurrentCultureIgnoreCase));

        if (existingUser != null && existingUser.Count != 0)
            throw new UsernameAlreadyTakenException();
    }
}