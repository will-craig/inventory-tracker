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
    /// Record one or more semantically supplied inventory additions for the agent user.
    /// </summary>
    [HttpPost("additions")]
    [SwaggerOperation(
        OperationId = "AddAgentInventory",
        Summary = "Add inventory in bulk for an agent",
        Description = "Creates inventory items from semantic agent input and returns a per-item outcome.")]
    [ProducesResponseType(typeof(InventoryAgentBulkWriteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<InventoryAgentBulkWriteResponse>> AddInventory(InventoryAgentBulkAddRequest request)
    {
        var userId = currentUserService.UserId;
        var username = currentUserService.Username;
        if (userId == null || username == null)
            return Unauthorized(ApiProblemDetails.Unauthorized());

        var additions = (request.Items ?? []).Select(item => new InventoryAgentAddition(
            item?.Name,
            item?.Quantity,
            item?.Unit,
            item?.ExpiryDate,
            item?.Category,
            item?.Location,
            item?.PurchasedDate,
            item?.OpenedDate,
            item?.Notes,
            item?.Reason)).ToList();

        var results = await inventoryService.AddInventoryForAgentAsync(userId, username, additions);
        return Ok(InventoryAgentMapper.MapBulkWriteResponse(results, DateTime.UtcNow.Date));
    }

    /// <summary>
    /// Apply one or more semantic inventory updates to existing agent-user items.
    /// </summary>
    [HttpPost("updates")]
    [SwaggerOperation(
        OperationId = "UpdateAgentInventory",
        Summary = "Update inventory in bulk for an agent",
        Description = "Applies semantic updates by stable item id and returns a per-item outcome.")]
    [ProducesResponseType(typeof(InventoryAgentBulkWriteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<InventoryAgentBulkWriteResponse>> UpdateInventory(InventoryAgentBulkUpdateRequest request)
    {
        var userId = currentUserService.UserId;
        if (userId == null)
            return Unauthorized(ApiProblemDetails.Unauthorized());

        var updates = (request.Items ?? []).Select(item => new InventoryAgentUpdate(
            item?.Id,
            item?.Name,
            item?.Quantity,
            item?.Unit,
            item?.ExpiryDate,
            item?.Category,
            item?.Location,
            item?.PurchasedDate,
            item?.OpenedDate,
            item?.Notes,
            item?.Reason)).ToList();

        var results = await inventoryService.UpdateInventoryForAgentAsync(userId, updates);
        return Ok(InventoryAgentMapper.MapBulkWriteResponse(results, DateTime.UtcNow.Date));
    }

    /// <summary>
    /// Consume a quantity of an inventory item, deleting it when the remaining quantity is zero.
    /// </summary>
    [HttpPost("items/{id}/consume")]
    [SwaggerOperation(
        OperationId = "ConsumeAgentInventoryItem",
        Summary = "Consume inventory for an agent",
        Description = "Subtracts consumed quantity from an owned item and deletes it if depleted.")]
    [ProducesResponseType(typeof(InventoryAgentItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryAgentItemResponse>> ConsumeInventoryItem(string id, ConsumeInventoryItemRequest request)
    {
        var userId = currentUserService.UserId;
        if (userId == null)
            return Unauthorized(ApiProblemDetails.Unauthorized());

        var result = await inventoryService.ConsumeInventoryForAgentAsync(new InventoryAgentConsumeCommand(
            id,
            userId,
            request.Quantity,
            request.Reason));

        return result.Status switch
        {
            "updated" when result.Item != null => Ok(InventoryAgentMapper.MapItemResponse(result.Item, DateTime.UtcNow.Date)),
            "deleted" => NoContent(),
            "failed" when result.Message == "Item not found." => NotFound(ApiProblemDetails.NotFound(result.Message)),
            "failed" when result.Message == "You do not have access to this item." => StatusCode(
                StatusCodes.Status403Forbidden,
                ApiProblemDetails.Forbidden(result.Message)),
            _ => BadRequest(ApiProblemDetails.BadRequest(result.Message ?? "The consume request is invalid."))
        };
    }

    /// <summary>
    /// Delete an inventory item owned by the agent user.
    /// </summary>
    [HttpDelete("items/{id}")]
    [SwaggerOperation(
        OperationId = "DeleteAgentInventoryItem",
        Summary = "Delete inventory for an agent",
        Description = "Deletes an owned inventory item by stable item id.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInventoryItem(string id)
    {
        var userId = currentUserService.UserId;
        if (userId == null)
            return Unauthorized(ApiProblemDetails.Unauthorized());

        var result = await inventoryService.DeleteInventoryForAgentAsync(id, userId);
        return result.Status switch
        {
            "deleted" => NoContent(),
            "failed" when result.Message == "Item not found." => NotFound(ApiProblemDetails.NotFound(result.Message)),
            "failed" when result.Message == "You do not have access to this item." => StatusCode(
                StatusCodes.Status403Forbidden,
                ApiProblemDetails.Forbidden(result.Message)),
            _ => BadRequest(ApiProblemDetails.BadRequest(result.Message ?? "The delete request is invalid."))
        };
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
