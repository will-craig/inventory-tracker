using System.Linq.Expressions;
using MongoDB.Driver;

namespace Stockpile.DAL.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> FindAsync(Expression<Func<T, bool>> filter);
    Task CreateAsync(T entity);
    Task UpdateAsync(string id, T entity);
    Task DeleteAsync(string id);
}
public abstract class Repository<T>(IMongoDatabase database, string collectionName) : IRepository<T> where T : class
{
    protected readonly IMongoCollection<T> Collection = database.GetCollection<T>(collectionName);

    public async Task<List<T>> GetAllAsync()
    {
        return await Collection.Find(_ => true).ToListAsync();
    }
    
    public async Task<T?> GetByIdAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> filter)
    {
        return await Collection.Find(filter).ToListAsync();
    }
    
    public async Task CreateAsync(T entity)
    {
        await Collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(string id, T entity) 
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        await Collection.ReplaceOneAsync(filter, entity);
    }

    public async Task DeleteAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        await Collection.DeleteOneAsync(filter);
    }
}
