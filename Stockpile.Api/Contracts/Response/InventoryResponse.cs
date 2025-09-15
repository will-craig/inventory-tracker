using Stockpile.Domain.Enums;

namespace Stockpile.Api.Contracts.Response;

public record InventoryItemResponse
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public float Quantity { get; set; }
    public Unit Unit { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
