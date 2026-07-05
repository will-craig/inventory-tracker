using System.ComponentModel.DataAnnotations;
using Stockpile.Domain.Enums;

namespace Stockpile.Api.Contracts.Requests;

public record InventoryItemRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Name must not be blank.")]
    public required string Name { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    public Unit Unit { get; set; } 

    public DateTime? ExpiryDate { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    public DateTime? PurchasedDate { get; set; }

    public DateTime? OpenedDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
