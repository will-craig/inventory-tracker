using Stockpile.Contracts.Requests;
using Stockpile.Contracts.Response;
using Stockpile.DAL.Models;

namespace Stockpile.Contracts.Mappers;

public static class InventoryItemMapper
{
    public static InventoryItem Map(this InventoryItemUpdateRequest request, string userId, string username)
    {
        return new InventoryItem
        {
            Id = request.Id,
            Name = request.Name,
            Quantity = request.Quantity,
            Unit = request.Unit,
            ExpiryDate = request.ExpiryDate,
            UserId = userId,
            Username = username
        };
    }

    public static InventoryItem Map(this InventoryItemCreateRequest request, string userId, string username)
    {
        return new InventoryItem
        {
            Name = request.Name,
            Quantity = request.Quantity,
            Unit = request.Unit,
            ExpiryDate = request.ExpiryDate,
            UserId = userId,
            Username = username
        };
    }
    
    public static InventoryItemResponse Map(this InventoryItem item)
    {
        return new InventoryItemResponse
        {
            Id = item.Id,
            Name = item.Name,
            Quantity = item.Quantity,
            Unit = item.Unit,
            ExpiryDate = item.ExpiryDate
        };
    }
}