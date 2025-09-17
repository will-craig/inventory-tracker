namespace Stockpile.Api.Configuration.Models;

public class DatabaseConfig
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
}