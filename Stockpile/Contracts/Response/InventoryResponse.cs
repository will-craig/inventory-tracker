using Stockpile.DAL.Models;
using Stockpile.DAL.Models.Enums;

namespace Stockpile.Contracts.Response;

public class InventoryItemResponse
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public float Quantity { get; set; }
    public Unit Unit { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
