namespace Stockpile.Api.Configuration.Models;

public class DatabaseInitializationConfig
{
    public const string SectionName = "DatabaseInitialization";

    public bool RunOnStartup { get; set; } = true;
    public bool SeedData { get; set; } = true;
    public bool EnsureIndexes { get; set; } = true;
}
