using Stockpile.Domain.Entities;
using Stockpile.Domain.Enums;

namespace Stockpile.Domain.Models;

public record InventoryAgentQuery(
    string UserId,
    string? Search,
    string? Category,
    string? Location,
    DateTime? ExpiresFrom,
    DateTime? ExpiresTo,
    bool IncludeNoExpiry,
    InventoryAgentSort Sort,
    bool Descending,
    int Limit);

public record InventoryDigestOptions(
    string UserId,
    DateTime AsOf,
    IReadOnlyList<int> WindowsDays,
    int LimitPerSection,
    bool IncludeNoExpiry);

public record InventoryAgentDigest(
    DateTime AsOf,
    IReadOnlyList<int> WindowsDays,
    IReadOnlyList<InventoryItem> Expired,
    IReadOnlyDictionary<int, IReadOnlyList<InventoryItem>> DueWithinWindows,
    IReadOnlyList<InventoryItem> NoExpiry)
{
    public InventoryDigestCounts Counts { get; init; } = new(0, new Dictionary<int, int>(), 0, 0);
    public IReadOnlyList<InventoryAgentHint> Hints { get; init; } = [];
}

public record InventoryDigestCounts(
    int Expired,
    IReadOnlyDictionary<int, int> DueWithinDays,
    int NoExpiry,
    int TotalActionable);

public record InventoryAgentHint(
    string Type,
    string Priority,
    string Message,
    IReadOnlyList<string> ItemIds);
