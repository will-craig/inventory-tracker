using System.ComponentModel.DataAnnotations;
using Stockpile.Domain.Enums;

namespace Stockpile.Api.Contracts.Requests;

/// <summary>
/// New inventory item details supplied by the inventory agent after a clear add-item instruction.
/// </summary>
public record InventoryAgentCreateItemRequest
{
    /// <summary>Human-readable item name, for example "Greek yogurt". Required and must not be blank.</summary>
    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Name must not be blank.")]
    public required string Name { get; set; }

    /// <summary>Available amount for the item. Defaults to 0 when omitted and cannot be negative.</summary>
    [Range(0, double.MaxValue)]
    public float Quantity { get; set; }

    /// <summary>Unit used with quantity, for example Part, Gram, Milliliter, or Kilogram.</summary>
    public Unit Unit { get; set; }

    /// <summary>UTC expiry date if known. Omit when the user has not supplied an expiry date.</summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>Optional category such as "Dairy", "Bakery", or "Produce".</summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>Optional storage location such as "Fridge", "Freezer", or "Cupboard".</summary>
    [MaxLength(100)]
    public string? Location { get; set; }

    /// <summary>UTC purchase date if known.</summary>
    public DateTime? PurchasedDate { get; set; }

    /// <summary>UTC opened date if known.</summary>
    public DateTime? OpenedDate { get; set; }

    /// <summary>Short optional notes from the user's instruction or source context.</summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// Partial inventory item update supplied by the inventory agent. Omitted fields are left unchanged.
/// </summary>
public record InventoryAgentUpdateItemRequest
{
    /// <summary>Replacement item name. Omit to keep the current name.</summary>
    [MinLength(1)]
    [MaxLength(200)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Name must not be blank.")]
    public string? Name { get; set; }

    /// <summary>Replacement available amount. Omit to keep the current quantity.</summary>
    [Range(0, double.MaxValue)]
    public float? Quantity { get; set; }

    /// <summary>Replacement unit. Omit to keep the current unit.</summary>
    public Unit? Unit { get; set; }

    /// <summary>Replacement UTC expiry date. Use clear rather than null to remove the current expiry date.</summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>Replacement category. Use clear rather than null to remove the current category.</summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>Replacement storage location. Use clear rather than null to remove the current location.</summary>
    [MaxLength(100)]
    public string? Location { get; set; }

    /// <summary>Replacement UTC purchase date. Use clear rather than null to remove the current purchase date.</summary>
    public DateTime? PurchasedDate { get; set; }

    /// <summary>Replacement UTC opened date. Use clear rather than null to remove the current opened date.</summary>
    public DateTime? OpenedDate { get; set; }

    /// <summary>Replacement notes. Use clear rather than null to remove the current notes.</summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Optional fields to remove explicitly. Supported values: expiryDate, category, location, purchasedDate,
    /// openedDate, notes.
    /// </summary>
    public List<string>? Clear { get; set; } = [];

    public bool HasChanges() =>
        Name is not null ||
        Quantity.HasValue ||
        Unit.HasValue ||
        ExpiryDate.HasValue ||
        Category is not null ||
        Location is not null ||
        PurchasedDate.HasValue ||
        OpenedDate.HasValue ||
        Notes is not null ||
        Clear?.Count > 0;
}

/// <summary>
/// Quantity consumed from an existing inventory item.
/// </summary>
public record ConsumeInventoryItemRequest
{
    /// <summary>Amount to subtract from the current item quantity. Defaults to 1 and must be greater than 0.</summary>
    public float Quantity { get; set; } = 1;

    /// <summary>Optional note to append to the item after consumption, for example "Used for dinner".</summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}
