using System.Text.Json;
using FluentAssertions;

namespace Stockpile.UnitTests.Configuration;

public class AgentLocalRuntimeConfigurationTests
{
    [Fact]
    public void LaunchSettings_DefinesAgentLocalLoopbackProfile()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(ProjectFile("Stockpile.Api/Properties/launchSettings.json")));
        var profile = document.RootElement
            .GetProperty("profiles")
            .GetProperty("AgentLocal");

        profile.GetProperty("launchBrowser").GetBoolean().Should().BeFalse();
        profile.GetProperty("applicationUrl").GetString().Should().Be("http://127.0.0.1:8080");

        var environment = profile.GetProperty("environmentVariables");
        environment.GetProperty("ASPNETCORE_ENVIRONMENT").GetString().Should().Be("AgentLocal");
        environment.GetProperty("ASPNETCORE_URLS").GetString().Should().Be("http://127.0.0.1:8080");
    }

    [Fact]
    public void AgentLocalSettings_EnableSwaggerJsonButDisableUiAndBlockingStartupWork()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(ProjectFile("Stockpile.Api/appsettings.AgentLocal.json")));

        var swagger = document.RootElement.GetProperty("Swagger");
        swagger.GetProperty("Enabled").GetBoolean().Should().BeFalse();
        swagger.GetProperty("JsonEnabled").GetBoolean().Should().BeTrue();
        swagger.GetProperty("UiEnabled").GetBoolean().Should().BeFalse();

        document.RootElement
            .GetProperty("AzureAd")
            .GetProperty("Enabled")
            .GetBoolean()
            .Should()
            .BeFalse();

        document.RootElement
            .GetProperty("DatabaseInitialization")
            .GetProperty("RunOnStartup")
            .GetBoolean()
            .Should()
            .BeFalse();
    }

    [Fact]
    public void Compose_KeepsApiAndToolsOutOfDefaultAgentRuntime()
    {
        var compose = File.ReadAllText(ProjectFile("compose.yaml"));

        compose.Should().Contain("stockpile-api:");
        compose.Should().Contain("profiles:");
        compose.Should().Contain("- api");
        compose.Should().Contain("restart: unless-stopped");
        compose.Should().Contain("- tools");
    }

    private static string ProjectFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Stockpile.sln")))
        {
            directory = directory.Parent;
        }

        directory.Should().NotBeNull("the tests should run from inside the repository");
        return Path.Combine(directory!.FullName, relativePath);
    }
}
