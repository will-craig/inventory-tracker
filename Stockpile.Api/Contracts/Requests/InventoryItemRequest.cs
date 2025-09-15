using Stockpile.Domain.Enums;

namespace Stockpile.Api.Contracts.Requests;

public record InventoryItemRequest
{
    public required string Name { get; set; }
    public int Quantity { get; set; }
    public Unit Unit { get; set; } 
    public DateTime? ExpiryDate { get; set; }
}
