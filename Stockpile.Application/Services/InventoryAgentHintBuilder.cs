using Stockpile.Domain.Models;

namespace Stockpile.Api.Services;

public static class InventoryAgentHintBuilder
{
    public static IReadOnlyList<InventoryAgentHint> Build(InventoryAgentDigest digest)
    {
        var hints = new List<InventoryAgentHint>();
        if (digest.Expired.Count > 0)
        {
            hints.Add(new InventoryAgentHint(
                "expired-items",
                "high",
                $"{digest.Expired.Count} item(s) are already expired. The agent should ask the user to inspect before using.",
                digest.Expired.Select(item => item.Id).ToList()));
        }

        var dueSoon = digest.DueWithinWindows.Values.SelectMany(items => items).ToList();
        if (dueSoon.Count > 0)
        {
            hints.Add(new InventoryAgentHint(
                "use-first",
                digest.Expired.Count > 0 ? "medium" : "high",
                $"{dueSoon.Count} item(s) should be prioritized before later inventory.",
                dueSoon.Select(item => item.Id).ToList()));
        }

        var lowQuantity = dueSoon.Where(item => item.Quantity <= 1).ToList();
        if (lowQuantity.Count > 0)
        {
            hints.Add(new InventoryAgentHint(
                "check-quantity",
                "medium",
                $"{lowQuantity.Count} urgent item(s) have quantity 1 or less; the agent should avoid assuming much is available.",
                lowQuantity.Select(item => item.Id).ToList()));
        }

        if (digest.NoExpiry.Count > 0)
        {
            hints.Add(new InventoryAgentHint(
                "missing-expiry",
                "low",
                $"{digest.NoExpiry.Count} item(s) have no expiry date and may need user confirmation.",
                digest.NoExpiry.Select(item => item.Id).ToList()));
        }

        return hints;
    }
}
