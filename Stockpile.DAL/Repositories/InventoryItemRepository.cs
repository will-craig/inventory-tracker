using MongoDB.Driver;
using Stockpile.Domain.Entities;

namespace Stockpile.DAL.Repositories;

public interface IInventoryItemRepository : IRepository<InventoryItem>
{
    Task UpdateUsernameOnCollection(string userId, string newUsername);
}
public class InventoryItemRepository(IMongoDatabase database) : Repository<InventoryItem>(database, nameof(InventoryItem)), IInventoryItemRepository
{
    public async Task UpdateUsernameOnCollection(string userId, string newUsername)
    {
        var filter = Builders<InventoryItem>.Filter.Eq(i => i.UserId, userId);
        var update = Builders<InventoryItem>.Update.Set(i => i.Username, newUsername);

        await Collection.UpdateManyAsync(filter, update);
    }
}