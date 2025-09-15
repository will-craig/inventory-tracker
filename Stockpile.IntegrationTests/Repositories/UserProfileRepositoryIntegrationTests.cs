using FluentAssertions;
using MongoDB.Driver;
using Stockpile.DAL.Repositories;
using Stockpile.Domain.Entities;

namespace Stockpile.IntegrationTests.Repositories;

public class UserProfileRepositoryIntegrationTests : IntegrationTestBase
{
    private readonly UserProfileRepository _repository;

    public UserProfileRepositoryIntegrationTests()
    {
        _repository = new UserProfileRepository(Database);
    }

    [Fact]
    public async Task GetByUsernameAsync_ReturnsInsertedUser()
    {
        var user = new UserProfile(MongoDB.Bson.ObjectId.GenerateNewId().ToString())
        {
            Username = "testuser",
            Email = "testuser@example.com",
            Preferences = new UserPreferences { DefaultUnits = Stockpile.Domain.Enums.Unit.Litre, NotificationsEnabled = true }
        };
        await _repository.CreateAsync(user);
        TrackCreatedId(user.Id);
        var result = await _repository.GetByUsernameAsync("testuser");
        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
        result.Email.Should().Be("testuser@example.com");
    }

    [Fact]
    public async Task GetByUsernameAsync_ReturnsNullForMissingUser()
    {
        var result = await _repository.GetByUsernameAsync("missinguser");
        result.Should().BeNull();
    }

    protected override async Task DeleteEntityAsync(string id)
    {
        await _repository.DeleteAsync(id);
    }
}
