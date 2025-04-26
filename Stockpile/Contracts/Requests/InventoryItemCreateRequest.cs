using Stockpile.DAL.Models;
using Stockpile.DAL.Models.Enums;

namespace Stockpile.Contracts.Requests;

public class InventoryItemCreateRequest
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public Unit Unit { get; set; } 
    public DateTime ExpiryDate { get; set; }
}
