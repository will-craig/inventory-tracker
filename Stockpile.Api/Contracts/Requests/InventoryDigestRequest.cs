using System.ComponentModel.DataAnnotations;

namespace Stockpile.Api.Contracts.Requests;

/// <summary>
/// Cron digest options for an inventory agent. If omitted, the API uses expired, 2-day, 5-day, and 10-day sections.
/// </summary>
public record InventoryDigestRequest
{
    /// <summary>UTC date used as "today" for expiry grouping. Defaults to the current UTC date.</summary>
    public DateTime? AsOf { get; set; }

    /// <summary>Maximum number of items in each digest section. Clamped to 1-100.</summary>
    [Range(1, 100)]
    public int LimitPerSection { get; set; } = 25;

    /// <summary>Include a compact section for items without expiry dates.</summary>
    public bool IncludeNoExpiry { get; set; } = true;
}