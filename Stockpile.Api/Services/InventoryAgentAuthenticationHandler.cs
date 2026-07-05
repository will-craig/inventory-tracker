using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Stockpile.Api.Configuration.Models;
using Stockpile.Domain.Enums;

namespace Stockpile.Api.Services;

public class InventoryAgentAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<InventoryAgentConfig> agentOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var config = agentOptions.Value;
        if (string.IsNullOrWhiteSpace(config.ApiKeySha256) ||
            string.IsNullOrWhiteSpace(config.UserId) ||
            string.IsNullOrWhiteSpace(config.Username))
        {
            return Task.FromResult(AuthenticateResult.Fail("Inventory agent authentication is not configured."));
        }

        if (!Request.Headers.TryGetValue(InventoryAgentConfig.HeaderName, out var suppliedKey) ||
            string.IsNullOrWhiteSpace(suppliedKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var suppliedHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(suppliedKey.ToString())));
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(suppliedHash),
                Encoding.UTF8.GetBytes(config.ApiKeySha256.ToUpperInvariant())))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid inventory agent API key."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, config.UserId),
            new Claim(ClaimTypes.Name, config.Username),
            new Claim(ClaimTypes.Role, nameof(UserRole.User))
        };

        var identity = new ClaimsIdentity(claims, InventoryAgentConfig.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, InventoryAgentConfig.AuthenticationScheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}