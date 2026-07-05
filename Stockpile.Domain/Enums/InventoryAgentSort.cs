using System.Text.Json.Serialization;

namespace Stockpile.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<InventoryAgentSort>))]
public enum InventoryAgentSort
{
    ExpiryDate = 0,
    Name = 1,
    Category = 2,
    Location = 3,
    CreatedAt = 4
}
