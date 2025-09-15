using Stockpile.DAL.Repositories;
using Stockpile.Domain.Entities;

namespace Stockpile.Api.Services;

public interface IInventoryService
{
    Task<InventoryItem?> GetInventoryItemAsync(string id);
    Task<List<InventoryItem>> GetInventoryItemsByUserAsync(string user);
    Task AddInventoryItemAsync(InventoryItem item);
    Task UpdateInventoryItemAsync(InventoryItem item);
    Task DeleteInventoryItemAsync(string id);
}

public class InventoryService(IInventoryItemRepository inventoryRepo) : IInventoryService
{
    public async Task<InventoryItem> GetInventoryItemAsync(string id)
    {
        return await inventoryRepo.GetByIdAsync(id);
    }
    
    public async Task<List<InventoryItem>> GetInventoryItemsByUserAsync(string user)
    {
        return await inventoryRepo.FindAsync(e => e.Username == user);
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