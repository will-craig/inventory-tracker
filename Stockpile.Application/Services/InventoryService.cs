using Stockpile.DAL.Repositories;
using Stockpile.Domain.Entities;
using Stockpile.Domain.Models;

namespace Stockpile.Api.Services;

public interface IInventoryService
{
    Task<InventoryItem?> GetInventoryItemAsync(string id);
    Task<List<InventoryItem>> GetInventoryItemsByUserAsync(string user);
    Task<List<InventoryItem>> QueryInventoryForAgentAsync(InventoryAgentQuery query);
    Task<InventoryAgentDigest> GetInventoryDigestForAgentAsync(InventoryDigestOptions options);
    Task AddInventoryItemAsync(InventoryItem item);
    Task UpdateInventoryItemAsync(InventoryItem item);
    Task DeleteInventoryItemAsync(string id);
}

public class InventoryService(IInventoryItemRepository inventoryRepo) : IInventoryService
{
    public async Task<InventoryItem?> GetInventoryItemAsync(string id)
    {
        return await inventoryRepo.GetByIdAsync(id);
    }
    
    public async Task<List<InventoryItem>> GetInventoryItemsByUserAsync(string user)
    {
        return await inventoryRepo.FindAsync(e => e.Username == user);
    }

    public async Task<List<InventoryItem>> QueryInventoryForAgentAsync(InventoryAgentQuery query)
    {
        var limit = Math.Clamp(query.Limit, 1, 200);
        return await inventoryRepo.QueryForAgentAsync(
            query.UserId,
            query.Search,
            query.Category,
            query.Location,
            query.ExpiresFrom,
            query.ExpiresTo,
            query.IncludeNoExpiry,
            query.Sort,
            query.Descending,
            limit);
    }

    public async Task<InventoryAgentDigest> GetInventoryDigestForAgentAsync(InventoryDigestOptions options)
    {
        var windows = options.WindowsDays
            .Where(days => days > 0)
            .Distinct()
            .Order()
            .ToArray();

        if (windows.Length == 0)
            windows = [2, 5, 10];

        var asOf = options.AsOf.Date;
        var maxExpiryDate = asOf.AddDays(windows.Max()).Date.AddDays(1).AddTicks(-1);
        var candidates = await inventoryRepo.GetDigestCandidatesAsync(options.UserId, maxExpiryDate, options.IncludeNoExpiry);
        var limit = Math.Clamp(options.LimitPerSection, 1, 100);

        var expired = candidates
            .Where(item => item.ExpiryDate.HasValue && item.ExpiryDate.Value.Date < asOf)
            .OrderBy(item => item.ExpiryDate)
            .ThenBy(item => item.Category)
            .ThenBy(item => item.Location)
            .ThenBy(item => item.Name)
            .Take(limit)
            .ToList();

        var dueWithinWindows = new Dictionary<int, IReadOnlyList<InventoryItem>>();
        var lowerBoundExclusive = asOf.AddDays(-1);
        foreach (var window in windows)
        {
            var upperBound = asOf.AddDays(window);
            var items = candidates
                .Where(item => item.ExpiryDate is not null &&
                               item.ExpiryDate.Value.Date > lowerBoundExclusive.Date &&
                               item.ExpiryDate.Value.Date <= upperBound.Date)
                .OrderBy(item => item.ExpiryDate)
                .ThenBy(item => item.Category)
                .ThenBy(item => item.Location)
                .ThenBy(item => item.Name)
                .Take(limit)
                .ToList();

            dueWithinWindows[window] = items;
            lowerBoundExclusive = upperBound;
        }

        var noExpiry = candidates
            .Where(item => item.ExpiryDate == null)
            .OrderBy(item => item.Category)
            .ThenBy(item => item.Location)
            .ThenBy(item => item.Name)
            .Take(limit)
            .ToList();

        var digest = new InventoryAgentDigest(asOf, windows, expired, dueWithinWindows, noExpiry);
        return digest with
        {
            Counts = new InventoryDigestCounts(
                expired.Count,
                dueWithinWindows.ToDictionary(pair => pair.Key, pair => pair.Value.Count),
                noExpiry.Count,
                expired.Count + dueWithinWindows.Values.Sum(items => items.Count)),
            Hints = InventoryAgentHintBuilder.Build(digest)
        };
    }

    public async Task AddInventoryItemAsync(InventoryItem item)
    {
        await inventoryRepo.CreateAsync(item);
    }

    public async Task UpdateInventoryItemAsync(InventoryItem item)
    {
        item.UpdatedAt = DateTime.UtcNow;
        await inventoryRepo.UpdateAsync(item.Id, item);
    }
    
    public async Task DeleteInventoryItemAsync(string id)
    {
        await inventoryRepo.DeleteAsync(id);
    }
    
}
