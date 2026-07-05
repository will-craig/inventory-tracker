namespace Stockpile.Api.Configuration.Models;

public class InventoryDigestConfig
{
    public const string SectionName = "InventoryDigest";

    public int[] DefaultExpiryWindowsDays { get; set; } = [2, 5, 10];
}