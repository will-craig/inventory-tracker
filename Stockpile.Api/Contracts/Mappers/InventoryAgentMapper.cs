using Stockpile.Api.Contracts.Requests;
using Stockpile.Api.Contracts.Response;
using Stockpile.Domain.Entities;
using Stockpile.Domain.Enums;
using Stockpile.Domain.Models;

namespace Stockpile.Api.Contracts.Mappers;

public static class InventoryAgentMapper
{
    private static readonly HashSet<string> ClearableFields = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(InventoryItem.ExpiryDate),
        nameof(InventoryItem.Category),
        nameof(InventoryItem.Location),
        nameof(InventoryItem.PurchasedDate),
        nameof(InventoryItem.OpenedDate),
        nameof(InventoryItem.Notes)
    };

    public static InventoryAgentQueryResponse MapQueryResponse(IEnumerable<InventoryItem> items, DateTime asOf)
    {
        var mappedItems = items.Select(item => MapItemResponse(item, asOf)).ToList();
        return new InventoryAgentQueryResponse
        {
            AsOf = asOf.Date,
            Count = mappedItems.Count,
            Items = mappedItems
        };
    }

    public static InventoryDigestResponse MapDigestResponse(InventoryAgentDigest digest)
    {
        var dueWithinDays = digest.DueWithinWindows.ToDictionary(
            pair => pair.Key.ToString(),
            pair => pair.Value.Select(item => MapItemResponse(item, digest.AsOf)).ToList());

        var response = new InventoryDigestResponse
        {
            AsOf = digest.AsOf.Date,
            WindowsDays = digest.WindowsDays.ToList(),
            Counts = MapCountsResponse(digest.Counts),
            Expired = digest.Expired.Select(item => MapItemResponse(item, digest.AsOf)).ToList(),
            DueWithinDays = dueWithinDays,
            NoExpiry = digest.NoExpiry.Select(item => MapItemResponse(item, digest.AsOf)).ToList(),
            Hints = digest.Hints.Select(MapHintResponse).ToList()
        };

        return response;
    }

    public static InventoryItem MapCreateItem(
        InventoryAgentCreateItemRequest request,
        string userId,
        string username)
    {
        return new InventoryItem
        {
            Name = request.Name,
            Quantity = request.Quantity,
            Unit = request.Unit,
            ExpiryDate = request.ExpiryDate,
            Category = request.Category,
            Location = request.Location,
            PurchasedDate = request.PurchasedDate,
            OpenedDate = request.OpenedDate,
            Notes = request.Notes,
            UserId = userId,
            Username = username
        };
    }

    public static void ApplyUpdate(InventoryItem item, InventoryAgentUpdateItemRequest request)
    {
        if (request.Name is not null)
            item.Name = request.Name;

        if (request.Quantity.HasValue)
            item.Quantity = request.Quantity.Value;

        if (request.Unit.HasValue)
            item.Unit = request.Unit.Value;

        if (request.ExpiryDate.HasValue)
            item.ExpiryDate = request.ExpiryDate.Value;

        if (request.Category is not null)
            item.Category = request.Category;

        if (request.Location is not null)
            item.Location = request.Location;

        if (request.PurchasedDate.HasValue)
            item.PurchasedDate = request.PurchasedDate.Value;

        if (request.OpenedDate.HasValue)
            item.OpenedDate = request.OpenedDate.Value;

        if (request.Notes is not null)
            item.Notes = request.Notes;

        foreach (var field in request.Clear ?? [])
        {
            switch (field.ToUpperInvariant())
            {
                case "EXPIRYDATE":
                    item.ExpiryDate = null;
                    break;
                case "CATEGORY":
                    item.Category = null;
                    break;
                case "LOCATION":
                    item.Location = null;
                    break;
                case "PURCHASEDDATE":
                    item.PurchasedDate = null;
                    break;
                case "OPENEDDATE":
                    item.OpenedDate = null;
                    break;
                case "NOTES":
                    item.Notes = null;
                    break;
            }
        }
    }

    public static bool HasUnsupportedClearFields(InventoryAgentUpdateItemRequest request, out string unsupportedField)
    {
        unsupportedField = request.Clear?.FirstOrDefault(field => !ClearableFields.Contains(field)) ?? string.Empty;
        return unsupportedField.Length > 0;
    }

    public static InventoryAgentItemResponse MapItemResponse(InventoryItem item, DateTime asOf)
    {
        var daysUntilExpiry = item.ExpiryDate.HasValue
            ? (int)(item.ExpiryDate.Value.Date - asOf.Date).TotalDays
            : (int?)null;

        return new InventoryAgentItemResponse
        {
            Id = item.Id,
            Name = item.Name,
            Quantity = item.Quantity,
            Unit = item.Unit,
            ExpiryDate = item.ExpiryDate,
            DaysUntilExpiry = daysUntilExpiry,
            Urgency = GetUrgency(daysUntilExpiry),
            Category = item.Category,
            Location = item.Location,
            PurchasedDate = item.PurchasedDate,
            OpenedDate = item.OpenedDate,
            Notes = item.Notes
        };
    }

    private static InventoryItemUrgency GetUrgency(int? daysUntilExpiry)
    {
        if (daysUntilExpiry == null)
            return InventoryItemUrgency.NoExpiry;

        if (daysUntilExpiry < 0)
            return InventoryItemUrgency.Expired;

        if (daysUntilExpiry == 0)
            return InventoryItemUrgency.DueToday;

        return daysUntilExpiry <= 10
            ? InventoryItemUrgency.DueSoon
            : InventoryItemUrgency.Later;
    }

    private static InventoryDigestCountsResponse MapCountsResponse(InventoryDigestCounts counts)
    {
        return new InventoryDigestCountsResponse
        {
            Expired = counts.Expired,
            DueWithinDays = counts.DueWithinDays.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value),
            NoExpiry = counts.NoExpiry,
            TotalActionable = counts.TotalActionable
        };
    }

    private static InventoryAgentHintResponse MapHintResponse(InventoryAgentHint hint)
    {
        return new InventoryAgentHintResponse
        {
            Type = hint.Type,
            Priority = hint.Priority,
            Message = hint.Message,
            ItemIds = hint.ItemIds.ToList()
        };
    }
}
