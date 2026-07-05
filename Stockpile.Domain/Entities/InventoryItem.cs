using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Stockpile.Domain.Enums;

namespace Stockpile.Domain.Entities;

public record InventoryItem(string id) : CollectionBase(id)
{
    public InventoryItem(): this(ObjectId.GenerateNewId().ToString()) { }
    
    public void SetId(string id) => Id = id;
    
    [BsonRequired]
    public required string Name { get; set; }

    [BsonDefaultValue(0)]
    public float Quantity { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public Unit Unit { get; set; } 
    
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? ExpiryDate { get; set; }

    public string? Category { get; set; }

    public string? Location { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? PurchasedDate { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? OpenedDate { get; set; }

    public string? Notes { get; set; }
    
    [BsonRepresentation(BsonType.ObjectId)]
    public required string UserId { get; set; }
    
    [BsonRequired]
    public required string Username { get; set; }
}