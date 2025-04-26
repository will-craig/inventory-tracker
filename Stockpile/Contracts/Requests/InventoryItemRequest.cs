using Stockpile.DAL.Models.Enums;

namespace Stockpile.Contracts.Requests;

public class InventoryItemRequest
{
    public required string Name { get; set; }
    public int Quantity { get; set; }
    public Unit Unit { get; set; } 
    public DateTime? ExpiryDate { get; set; }
}
