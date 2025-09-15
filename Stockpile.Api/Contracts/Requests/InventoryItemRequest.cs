using Stockpile.Domain.Enums;

namespace Stockpile.Api.Contracts.Requests;

public class InventoryItemRequest
{
    public required string Name { get; set; }
    public int Quantity { get; set; }
    public Unit Unit { get; set; } 
    public DateTime? ExpiryDate { get; set; }
}
