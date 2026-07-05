using System.ComponentModel.DataAnnotations;
using Stockpile.Domain.Enums;

namespace Stockpile.Api.Contracts.Requests;

/// <summary>
/// Server-side inventory filters for an inventory agent. Defaults return dated inventory ordered by earliest expiry.
/// </summary>
public record InventoryAgentQueryRequest
{
    /// <summary>Case-insensitive search across name, category, location, and notes.</summary>
    public string? Search { get; set; }

    /// <summary>Case-insensitive exact category filter, for example "Dairy".</summary>
    public string? Category { get; set; }

    /// <summary>Case-insensitive exact location filter, for example "Fridge".</summary>
    public string? Location { get; set; }

    /// <summary>Inclusive UTC expiry lower bound.</summary>
    public DateTime? ExpiresFrom { get; set; }

    /// <summary>Inclusive UTC expiry upper bound.</summary>
    public DateTime? ExpiresTo { get; set; }

    /// <summary>Include items that do not have an expiry date. Defaults to false to reduce agent context size.</summary>
    public bool IncludeNoExpiry { get; set; }

    /// <summary>Server-side sort field. Defaults to earliest expiry first.</summary>
    public InventoryAgentSort Sort { get; set; } = InventoryAgentSort.ExpiryDate;

    /// <summary>Reverse the selected sort. Defaults to false.</summary>
    public bool Descending { get; set; }

    /// <summary>Maximum number of items returned. Clamped to 1-200.</summary>
    [Range(1, 200)]
    public int Limit { get; set; } = 50;
}
