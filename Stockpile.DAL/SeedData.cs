using MongoDB.Bson;
using MongoDB.Driver;
using Stockpile.Domain.Entities;
using YamlDotNet.Serialization;

namespace Stockpile.DAL;

public static class SeedData
{
    public static void Initialize(IMongoDatabase db)
    {
        var userCollection = db.GetCollection<UserProfile>(nameof(UserProfile));
        if(userCollection.AsQueryable().Any())
            return;
        
        var seedData = LoadSeedData("seed-data.yaml");
        foreach (var data in seedData.Collection)
        {
            var testUser = SeedUsers(db, data.UserProfile);
            SeedInventoryItems(db, data.Inventory, testUser);
        }
    }
    
    private static SeedDataFile LoadSeedData(string filePath)
    {
        var yaml = File.ReadAllText(filePath);
        var deserializer = new DeserializerBuilder().Build();
        var seedData = deserializer.Deserialize<SeedDataFile>(yaml);
        
        if(seedData == null)
            throw new InvalidOperationException("Failed to load seed data from YAML file.");
        
        return seedData;
    }

    private static UserProfile SeedUsers(IMongoDatabase db, UserProfile userProfile)
    {
        var collection = db.GetCollection<UserProfile>(nameof(UserProfile));
        var user = collection.Find(e => e.Username == userProfile.Username).FirstOrDefault();
        if (user != null) 
            return null;
        
        var newUser = new UserProfile
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Username = userProfile.Username,
            Email = userProfile.Email,
            Preferences = userProfile.Preferences
        };
        collection.InsertOne(userProfile);
        return newUser;
    }
    
    private static void SeedInventoryItems(IMongoDatabase db, List<InventoryItem> groupInventoryItems, UserProfile testUser)
    {
        if (groupInventoryItems.Count == 0)
            return;
        
        var inventoryItemCollection = db.GetCollection<InventoryItem>(nameof(InventoryItem));
        groupInventoryItems.ForEach(item =>
        {
            item.UserId = testUser.Id;
            item.Username = testUser.Username;
            item.ExpiryDate = DateTime.Now.AddDays(1);
        });
        inventoryItemCollection.InsertMany(groupInventoryItems);
    }
}

public class SeedDataFile
{
    public List<SeedCollection> Collection { get; set; } = new();
}

public class SeedCollection
{
    public UserProfile UserProfile { get; set; } = default!;
    public List<InventoryItem> Inventory { get; set; } = new();
}