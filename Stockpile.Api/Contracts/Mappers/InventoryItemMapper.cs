using Stockpile.Api.Contracts.Requests;
using Stockpile.Api.Contracts.Response;
using Stockpile.Domain.Entities;

namespace Stockpile.Api.Contracts.Mappers;

public static class InventoryItemMapper
{
    public static InventoryItem MapTo(this InventoryItemRequest request, string id, string userId, string username)
    {
        return new InventoryItem
        {
            Id = id,
            Name = request.Name,
            Quantity = request.Quantity,
            Unit = request.Unit,
            ExpiryDate = request.ExpiryDate,
            UserId = userId,
            Username = username
        };
    }

    public static InventoryItem MapTo(this InventoryItemRequest request, string userId, string username)
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
    
    public static InventoryItemResponse MapFrom(this InventoryItem item)
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