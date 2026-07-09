using FluentAssertions;
using Stockpile.Api.Contracts.Mappers;
using Stockpile.Api.Contracts.Requests;
using Stockpile.Domain.Entities;
using Stockpile.Domain.Enums;

namespace Stockpile.UnitTests.Mappers;

[Collection("Mapper Tests")]
public class UserProfileMapperTests
{
    [Fact]
    public void MapTo_MapsRequestToEntityCorrectly()
    {
        var request = new UserProfileRequest
        {
            Id = "12345",
            Username = "DemoUser",
            Email = "demo@example.com",
            Preferences = new UserPreferences { DefaultUnits = Unit.Litre, NotificationsEnabled = true }
        };
        var entity = request.MapTo();
        entity.Id.Should().Be(request.Id);
        entity.Username.Should().Be(request.Username);
        entity.Email.Should().Be(request.Email);
        entity.Preferences.DefaultUnits.Should().Be(request.Preferences.DefaultUnits);
        entity.Preferences.NotificationsEnabled.Should().Be(request.Preferences.NotificationsEnabled);
    }

    [Fact]
    public void MapFrom_MapsEntityToResponseCorrectly()
    {
        var entity = new UserProfile
        {
            Username = "AnotherUser",
            Email = "another@example.com",
            Preferences = new UserPreferences { DefaultUnits = Unit.Part, NotificationsEnabled = false }
        };
        var response = entity.MapFrom();
        response.Username.Should().Be(entity.Username);
        response.Email.Should().Be(entity.Email);
        response.Preferences.Should().NotBeNull();
        response.Preferences!.DefaultUnits.Should().Be(entity.Preferences.DefaultUnits);
        response.Preferences.NotificationsEnabled.Should().Be(entity.Preferences.NotificationsEnabled);
    }
}
