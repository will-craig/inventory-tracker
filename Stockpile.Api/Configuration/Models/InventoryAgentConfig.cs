namespace Stockpile.Api.Configuration.Models;

public class InventoryAgentConfig
{
    public const string SectionName = "InventoryAgent";
    public const string AuthenticationScheme = "InventoryAgent";
    public const string PolicyName = "InventoryAgent";
    public const string HeaderName = "X-Inventory-Agent-Key";

    public string ApiKeySha256 { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}