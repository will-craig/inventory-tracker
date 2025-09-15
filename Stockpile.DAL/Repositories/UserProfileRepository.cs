using MongoDB.Driver;
using Stockpile.Domain.Entities;

namespace Stockpile.DAL.Repositories;

public interface IUserProfileRepository : IRepository<UserProfile>
{
    Task<UserProfile?> GetByUsernameAsync(string user);
}
public class UserProfileRepository(IMongoDatabase database) : Repository<UserProfile>(database, nameof(UserProfile)), IUserProfileRepository 
{
    public async Task<UserProfile?> GetByUsernameAsync(string user)
    {
        var filter = Builders<UserProfile>.Filter.Eq(u => u.Username, user);
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }
}