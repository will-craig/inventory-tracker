namespace Stockpile.Contracts.Requests;

public class InventoryItemUpdateRequest : InventoryItemCreateRequest
{
    public string Id { get; set; }
}
