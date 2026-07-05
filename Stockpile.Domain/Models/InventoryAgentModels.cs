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
    IReadOnlyList<InventoryItem> NoExpiry);
