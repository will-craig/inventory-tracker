using MongoDB.Driver;
using Stockpile.Domain.Entities;
using Stockpile.Domain.Enums;
using System.Text.RegularExpressions;

namespace Stockpile.DAL.Repositories;

public interface IInventoryItemRepository : IRepository<InventoryItem>
{
    Task UpdateUsernameOnCollection(string userId, string newUsername);
    Task<List<InventoryItem>> QueryForAgentAsync(
        string userId,
        string? search,
        string? category,
        string? location,
        DateTime? expiresFrom,
        DateTime? expiresTo,
        bool includeNoExpiry,
        InventoryAgentSort sort,
        bool descending,
        int limit);
    Task<List<InventoryItem>> GetDigestCandidatesAsync(string userId, DateTime maxExpiryDate, bool includeNoExpiry);
    Task EnsureAgentIndexesAsync();
}
public class InventoryItemRepository(IMongoDatabase database) : Repository<InventoryItem>(database, nameof(InventoryItem)), IInventoryItemRepository
{
    public async Task UpdateUsernameOnCollection(string userId, string newUsername)
    {
        var filter = Builders<InventoryItem>.Filter.Eq(i => i.UserId, userId);
        var update = Builders<InventoryItem>.Update.Set(i => i.Username, newUsername);

        await Collection.UpdateManyAsync(filter, update);
    }

    public async Task<List<InventoryItem>> QueryForAgentAsync(
        string userId,
        string? search,
        string? category,
        string? location,
        DateTime? expiresFrom,
        DateTime? expiresTo,
        bool includeNoExpiry,
        InventoryAgentSort sort,
        bool descending,
        int limit)
    {
        var filter = BuildAgentFilter(userId, search, category, location, expiresFrom, expiresTo, includeNoExpiry);
        var sortDefinition = BuildAgentSort(sort, descending);

        return await Collection
            .Find(filter)
            .Sort(sortDefinition)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<List<InventoryItem>> GetDigestCandidatesAsync(string userId, DateTime maxExpiryDate, bool includeNoExpiry)
    {
        var builder = Builders<InventoryItem>.Filter;
        var expiringFilter = builder.And(
            builder.Eq(item => item.UserId, userId),
            builder.Ne(item => item.ExpiryDate, null),
            builder.Lte(item => item.ExpiryDate, maxExpiryDate));

        var filter = includeNoExpiry
            ? builder.Or(expiringFilter, builder.And(
                builder.Eq(item => item.UserId, userId),
                builder.Eq(item => item.ExpiryDate, null)))
            : expiringFilter;

        return await Collection
            .Find(filter)
            .ToListAsync();
    }

    public async Task EnsureAgentIndexesAsync()
    {
        var indexes = new[]
        {
            new CreateIndexModel<InventoryItem>(
                Builders<InventoryItem>.IndexKeys.Ascending(item => item.UserId).Ascending(item => item.ExpiryDate)),
            new CreateIndexModel<InventoryItem>(
                Builders<InventoryItem>.IndexKeys.Ascending(item => item.UserId).Ascending(item => item.Category)),
            new CreateIndexModel<InventoryItem>(
                Builders<InventoryItem>.IndexKeys.Ascending(item => item.UserId).Ascending(item => item.Location)),
            new CreateIndexModel<InventoryItem>(
                Builders<InventoryItem>.IndexKeys.Ascending(item => item.UserId).Ascending(item => item.Name))
        };

        await Collection.Indexes.CreateManyAsync(indexes);
    }

    private static FilterDefinition<InventoryItem> BuildAgentFilter(
        string userId,
        string? search,
        string? category,
        string? location,
        DateTime? expiresFrom,
        DateTime? expiresTo,
        bool includeNoExpiry)
    {
        var builder = Builders<InventoryItem>.Filter;
        var filters = new List<FilterDefinition<InventoryItem>>
        {
            builder.Eq(item => item.UserId, userId)
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var regex = new MongoDB.Bson.BsonRegularExpression(Regex.Escape(search.Trim()), "i");
            filters.Add(builder.Or(
                builder.Regex(item => item.Name, regex),
                builder.Regex(item => item.Category, regex),
                builder.Regex(item => item.Location, regex),
                builder.Regex(item => item.Notes, regex)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            filters.Add(builder.Regex(item => item.Category,
                new MongoDB.Bson.BsonRegularExpression($"^{Regex.Escape(category.Trim())}$", "i")));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            filters.Add(builder.Regex(item => item.Location,
                new MongoDB.Bson.BsonRegularExpression($"^{Regex.Escape(location.Trim())}$", "i")));
        }

        if (expiresFrom.HasValue)
            filters.Add(builder.Gte(item => item.ExpiryDate, expiresFrom.Value));

        if (expiresTo.HasValue)
            filters.Add(builder.Lte(item => item.ExpiryDate, expiresTo.Value));

        if (!includeNoExpiry)
            filters.Add(builder.Ne(item => item.ExpiryDate, null));

        return builder.And(filters);
    }

    private static SortDefinition<InventoryItem> BuildAgentSort(InventoryAgentSort sort, bool descending)
    {
        var builder = Builders<InventoryItem>.Sort;
        var primary = sort switch
        {
            InventoryAgentSort.Name => descending
                ? builder.Descending(item => item.Name)
                : builder.Ascending(item => item.Name),
            InventoryAgentSort.Category => descending
                ? builder.Descending(item => item.Category)
                : builder.Ascending(item => item.Category),
            InventoryAgentSort.Location => descending
                ? builder.Descending(item => item.Location)
                : builder.Ascending(item => item.Location),
            InventoryAgentSort.CreatedAt => descending
                ? builder.Descending(item => item.CreatedAt)
                : builder.Ascending(item => item.CreatedAt),
            _ => descending
                ? builder.Descending(item => item.ExpiryDate)
                : builder.Ascending(item => item.ExpiryDate)
        };

        return builder.Combine(primary, builder.Ascending(item => item.Name));
    }
}