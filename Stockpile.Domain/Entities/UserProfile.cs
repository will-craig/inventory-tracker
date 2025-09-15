using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Stockpile.Domain.Enums;

namespace Stockpile.Domain.Entities;

public record UserProfile(string id) : CollectionBase(id)
{
    /// <summary>
    /// Parameterless constructor will create a new Id
    /// </summary>
    public UserProfile(): this(ObjectId.GenerateNewId().ToString()) { }
    
    [BsonRequired]
    [BsonElement("username")]
    public required string Username { get; set; }

    [BsonRequired]
    [BsonElement("email")]
    public required string Email { get; set; }

    [BsonElement("preferences")]
    public UserPreferences Preferences { get; set; } = null!;
    
    [BsonRequired]
    [BsonElement("role")]
    public UserRole UserRole { get; set; } = UserRole.User;
}

public class UserPreferences
{
    [BsonElement("defaultUnits")]
    public Unit DefaultUnits { get; set; } = Unit.Part; // enum e.g., grams, oz, lbs

    [BsonElement("notifications")]
    public bool NotificationsEnabled { get; set; } = true;
}