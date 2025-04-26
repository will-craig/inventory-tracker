using MongoDB.Bson;
using Stockpile.DAL.Models.Enums;

namespace Stockpile.DAL.Models;

using MongoDB.Bson.Serialization.Attributes;

public class UserProfile(string id) : CollectionBase(id)
{
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
    public Role Role { get; set; } = Role.User;
}

public class UserPreferences
{
    [BsonElement("defaultUnits")]
    public Unit DefaultUnits { get; set; } = Unit.Part; // enum e.g., grams, oz, lbs

    [BsonElement("notifications")]
    public bool NotificationsEnabled { get; set; } = true;
}