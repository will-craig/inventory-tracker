using System.Text.Json.Serialization;

namespace Stockpile.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<InventoryItemUrgency>))]
public enum InventoryItemUrgency
{
    Expired = 0,
    DueToday = 1,
    DueSoon = 2,
    Later = 3,
    NoExpiry = 4
}