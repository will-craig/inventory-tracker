using Stockpile.Domain.Enums;

namespace Stockpile.Api.Contracts.Response;

/// <summary>
/// Compact inventory item optimized for agent context.
/// </summary>
public record InventoryAgentItemResponse
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public float Quantity { get; set; }
    public Unit Unit { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? DaysUntilExpiry { get; set; }
    public InventoryItemUrgency Urgency { get; set; }
    public string? Category { get; set; }
    public string? Location { get; set; }
    public DateTime? PurchasedDate { get; set; }
    public DateTime? OpenedDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Query result with server-side filters already applied.
/// </summary>
public record InventoryAgentQueryResponse
{
    public DateTime AsOf { get; set; }
    public int Count { get; set; }
    public List<InventoryAgentItemResponse> Items { get; set; } = [];
}

/// <summary>
/// Inventory agent digest with grouped expiry sections and deterministic hints.
/// </summary>
public record InventoryDigestResponse
{
    public DateTime AsOf { get; set; }
    public List<int> WindowsDays { get; set; } = [];
    public InventoryDigestCountsResponse Counts { get; set; } = new();
    public List<InventoryAgentItemResponse> Expired { get; set; } = [];
    public Dictionary<string, List<InventoryAgentItemResponse>> DueWithinDays { get; set; } = new();
    public List<InventoryAgentItemResponse> NoExpiry { get; set; } = [];
    public List<InventoryAgentHintResponse> Hints { get; set; } = [];
}

public record InventoryDigestCountsResponse
{
    public int Expired { get; set; }
    public Dictionary<string, int> DueWithinDays { get; set; } = new();
    public int NoExpiry { get; set; }
    public int TotalActionable { get; set; }
}

public record InventoryAgentHintResponse
{
    public required string Type { get; set; }
    public required string Priority { get; set; }
    public required string Message { get; set; }
    public List<string> ItemIds { get; set; } = [];
}
