using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stockpile.Api.Configuration.Models;
using Stockpile.Api.Contracts.Mappers;
using Stockpile.Api.Contracts.Requests;
using Stockpile.Api.Contracts.Response;
using Stockpile.Api.Services;
using Stockpile.Domain.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Stockpile.Api.Controllers;

[ApiController]
[Authorize(Policy = InventoryAgentConfig.PolicyName)]
[Route("api/agent/inventory")]
[Produces("application/json")]
public class AgentInventoryController(
    IInventoryService inventoryService,
    ICurrentUserService currentUserService,
    IOptions<InventoryDigestConfig> digestOptions) : ControllerBase
{
    /// <summary>
    /// Query inventory with agent-friendly server-side filtering and sorting.
    /// </summary>
    /// <remarks>
    /// The inventory agent should use this endpoint for targeted follow-up queries after the cron digest.
    /// Authenticate with the raw API key in the X-Inventory-Agent-Key header.
    ///
    /// Example:
    /// GET /api/agent/inventory/query?expiresTo=2026-07-06&amp;category=Dairy&amp;limit=10
    /// </remarks>
    [HttpGet("query")]
    [SwaggerOperation(
        OperationId = "QueryAgentInventory",
        Summary = "Query inventory for an agent",
        Description = "Returns a compact list of inventory items after applying server-side filters. Defaults exclude no-expiry items and sort by earliest expiry.")]
    [ProducesResponseType(typeof(InventoryAgentQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<InventoryAgentQueryResponse>> QueryInventory([FromQuery] InventoryAgentQueryRequest request)
    {
        if (currentUserService.UserId == null)
            return Unauthorized(ApiProblemDetails.Unauthorized());

        var asOf = DateTime.UtcNow.Date;
        var items = await inventoryService.QueryInventoryForAgentAsync(new InventoryAgentQuery(
            currentUserService.UserId,
            request.Search,
            request.Category,
            request.Location,
            request.ExpiresFrom,
            request.ExpiresTo,
            request.IncludeNoExpiry,
            request.Sort,
            request.Descending,
            request.Limit));

        return Ok(InventoryAgentMapper.MapQueryResponse(items, asOf));
    }

    /// <summary>
    /// Get the compact cron digest the agent should use for routine inventory updates.
    /// </summary>
    /// <remarks>
    /// This is the primary inventory agent endpoint. It groups items into expired, due-in-window, and no-expiry sections,
    /// then adds deterministic hints so the agent can produce an update without scanning the full inventory.
    /// Authenticate with the raw API key in the X-Inventory-Agent-Key header.
    ///
    /// Default windows are expired, due within 2 days, due within 5 days, and due within 10 days.
    ///
    /// Example:
    /// GET /api/agent/inventory/digest?limitPerSection=15&amp;includeNoExpiry=true
    /// </remarks>
    [HttpGet("digest")]
    [SwaggerOperation(
        OperationId = "GetAgentInventoryDigest",
        Summary = "Get inventory agent digest",
        Description = "Returns grouped expiring inventory and structured hints for an inventory agent. Uses configured default windows of 2, 5, and 10 days.")]
    [ProducesResponseType(typeof(InventoryDigestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<InventoryDigestResponse>> GetDigest([FromQuery] InventoryDigestRequest request)
    {
        if (currentUserService.UserId == null)
            return Unauthorized(ApiProblemDetails.Unauthorized());

        var config = digestOptions.Value;
        var digest = await inventoryService.GetInventoryDigestForAgentAsync(new InventoryDigestOptions(
            currentUserService.UserId,
            request.AsOf?.Date ?? DateTime.UtcNow.Date,
            config.DefaultExpiryWindowsDays,
            request.LimitPerSection,
            request.IncludeNoExpiry));

        return Ok(InventoryAgentMapper.MapDigestResponse(digest));
    }
}
