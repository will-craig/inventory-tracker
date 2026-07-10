using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stockpile.Api.Configuration.Models;
using Stockpile.Api.Contracts.Mappers;
using Stockpile.Api.Contracts.Requests;
using Stockpile.Api.Contracts.Response;
using Stockpile.Api.Services;
using Stockpile.Domain.Entities;
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
    private const string AcceptQueryHeaderName = "Accept-Query";
    private const string QueryMediaType = "application/json";

    /// <summary>
    /// Query inventory with agent-friendly server-side filtering and sorting.
    /// </summary>
    /// <remarks>
    /// The inventory agent should use this endpoint for targeted follow-up queries after the cron digest.
    /// Authenticate with the raw API key in the X-Inventory-Agent-Key header.
    ///
    /// Example:
    /// GET /api/agent/inventory/query?expiresTo=2026-07-06&amp;category=Dairy&amp;limit=10
    ///
    /// RFC 10008 QUERY is also supported at this path with an application/json body. Use GET as the compatibility
    /// fallback when clients or intermediaries do not support QUERY.
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
        return await QueryInventoryCore(request);
    }

    /// <summary>
    /// Query inventory with a safe, idempotent HTTP QUERY request body.
    /// </summary>
    /// <remarks>
    /// This is an additive RFC 10008-compatible alternative to the GET query endpoint. Use the GET endpoint as a
    /// compatibility fallback when clients or intermediaries do not support QUERY.
    ///
    /// Example:
    /// QUERY /api/agent/inventory/query
    /// Content-Type: application/json
    ///
    /// { "expiresTo": "2026-07-06", "category": "Dairy", "limit": 10 }
    /// </remarks>
    [AcceptVerbs("QUERY", Route = "query")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Consumes(QueryMediaType)]
    [ProducesResponseType(typeof(InventoryAgentQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<InventoryAgentQueryResponse>> QueryInventoryWithBody(
        [FromBody] InventoryAgentQueryRequest request)
    {
        return await QueryInventoryCore(request);
    }

    private async Task<ActionResult<InventoryAgentQueryResponse>> QueryInventoryCore(InventoryAgentQueryRequest request)
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

        Response.Headers[AcceptQueryHeaderName] = QueryMediaType;
        return Ok(InventoryAgentMapper.MapQueryResponse(items, asOf));
    }

    /// <summary>
    /// Insert an inventory item from an agent instruction.
    /// </summary>
    /// <remarks>
    /// Use this endpoint only when the user has clearly asked to add a new inventory item or confirmed that an item is
    /// not already represented in inventory. For ambiguous instructions, query first by name/category/location so the
    /// agent does not create duplicates.
    /// Authenticate with the raw API key in the X-Inventory-Agent-Key header.
    ///
    /// Required fields:
    /// name. Quantity defaults to 0 when omitted, and unit defaults to None when omitted.
    ///
    /// Example:
    /// POST /api/agent/inventory/items
    /// Content-Type: application/json
    ///
    /// {
    ///   "name": "Greek yogurt",
    ///   "quantity": 2,
    ///   "unit": "Part",
    ///   "expiryDate": "2026-07-12T00:00:00Z",
    ///   "category": "Dairy",
    ///   "location": "Fridge",
    ///   "notes": "Added from grocery receipt"
    /// }
    ///
    /// Response is the compact agent item shape, including the generated id. Use that id for later update or consume
    /// calls. The API assigns the configured inventory-agent user to the new item; the agent must not send user fields.
    /// </remarks>
    [HttpPost("items")]
    [SwaggerOperation(
        OperationId = "CreateAgentInventoryItem",
        Summary = "Create inventory item for an agent",
        Description = "Creates a new inventory item for clear add-item instructions. Query first when the instruction might refer to an existing item.")]
    [ProducesResponseType(typeof(InventoryAgentItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<InventoryAgentItemResponse>> CreateItem(InventoryAgentCreateItemRequest request)
    {
        var userId = currentUserService.UserId;
        var username = currentUserService.Username;
        if (userId == null || username == null)
            return Unauthorized(ApiProblemDetails.Unauthorized());

        var item = InventoryAgentMapper.MapCreateItem(request, userId, username);
        await inventoryService.AddInventoryItemAsync(item);

        return Created(
            $"/api/agent/inventory/items/{item.Id}",
            InventoryAgentMapper.MapItemResponse(item, DateTime.UtcNow.Date));
    }

    /// <summary>
    /// Apply a partial inventory item update from an agent instruction.
    /// </summary>
    /// <remarks>
    /// Use this endpoint when the user wants to correct or enrich an existing item, for example changing quantity,
    /// category, location, expiry date, opened date, or notes. The agent should identify the item id with the query or
    /// digest endpoint before calling this endpoint.
    /// Authenticate with the raw API key in the X-Inventory-Agent-Key header.
    ///
    /// Only send fields that should change. Omitted fields are left unchanged. To remove an optional value, include the
    /// property name in clear. Supported clear values are: expiryDate, category, location, purchasedDate, openedDate,
    /// notes. Do not use null to clear a field; use clear so the instruction is explicit.
    ///
    /// Example:
    /// PATCH /api/agent/inventory/items/64f1f0a9978a9b0f1a111111
    /// Content-Type: application/json
    ///
    /// {
    ///   "quantity": 1,
    ///   "location": "Freezer",
    ///   "clear": [ "openedDate", "notes" ]
    /// }
    ///
    /// The API rejects empty patch documents and unsupported clear fields with 400. It returns 404 when the id does not
    /// exist and 403 when the item belongs to another user. Response is the updated compact agent item.
    /// </remarks>
    [HttpPatch("items/{id}")]
    [SwaggerOperation(
        OperationId = "UpdateAgentInventoryItem",
        Summary = "Update inventory item for an agent",
        Description = "Applies partial item changes for agent instructions. Omitted fields stay unchanged; use clear to explicitly remove optional values.")]
    [ProducesResponseType(typeof(InventoryAgentItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryAgentItemResponse>> UpdateItem(
        string id,
        InventoryAgentUpdateItemRequest request)
    {
        var (item, error) = await GetOwnedItem(id);
        if (error is not null)
            return error;

        if (!request.HasChanges())
            return BadRequest(ApiProblemDetails.BadRequest("At least one update field or clear field is required."));

        if (InventoryAgentMapper.HasUnsupportedClearFields(request, out var unsupportedField))
            return BadRequest(ApiProblemDetails.BadRequest($"Cannot clear unsupported field '{unsupportedField}'."));

        InventoryAgentMapper.ApplyUpdate(item, request);
        await inventoryService.UpdateInventoryItemAsync(item);

        return Ok(InventoryAgentMapper.MapItemResponse(item, DateTime.UtcNow.Date));
    }

    /// <summary>
    /// Consume part of an inventory item by decrementing its quantity.
    /// </summary>
    /// <remarks>
    /// Use this endpoint when the user indicates that some of an existing item was used, eaten, cooked, discarded, or
    /// otherwise consumed. The agent should identify the item id with the query or digest endpoint before calling this
    /// endpoint.
    /// Authenticate with the raw API key in the X-Inventory-Agent-Key header.
    ///
    /// Consumption subtracts quantity from the current item and keeps the item even when the resulting quantity is 0.
    /// It does not delete the item. The API rejects zero, negative, and over-available quantities with 400 so the agent
    /// can ask the user to clarify instead of guessing.
    ///
    /// Example:
    /// POST /api/agent/inventory/items/64f1f0a9978a9b0f1a111111/consume
    /// Content-Type: application/json
    ///
    /// {
    ///   "quantity": 0.5,
    ///   "notes": "Used for dinner"
    /// }
    ///
    /// Optional notes are appended to the existing notes. Response is the updated compact agent item.
    /// </remarks>
    [HttpPost("items/{id}/consume")]
    [SwaggerOperation(
        OperationId = "ConsumeAgentInventoryItem",
        Summary = "Consume inventory item for an agent",
        Description = "Subtracts a consumed amount from an existing item. The item remains in inventory at quantity 0 rather than being deleted.")]
    [ProducesResponseType(typeof(InventoryAgentItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryAgentItemResponse>> ConsumeItem(
        string id,
        ConsumeInventoryItemRequest request)
    {
        var (item, error) = await GetOwnedItem(id);
        if (error is not null)
            return error;

        if (request.Quantity <= 0)
            return BadRequest(ApiProblemDetails.BadRequest("Consumed quantity must be greater than zero."));

        if (request.Quantity > item.Quantity)
            return BadRequest(ApiProblemDetails.BadRequest("Consumed quantity cannot exceed available quantity."));

        item.Quantity -= request.Quantity;
        if (!string.IsNullOrWhiteSpace(request.Notes))
            item.Notes = string.IsNullOrWhiteSpace(item.Notes)
                ? request.Notes
                : $"{item.Notes}\n{request.Notes}";

        await inventoryService.UpdateInventoryItemAsync(item);
        return Ok(InventoryAgentMapper.MapItemResponse(item, DateTime.UtcNow.Date));
    }

    private async Task<(InventoryItem Item, ActionResult? Error)> GetOwnedItem(string id)
    {
        var userId = currentUserService.UserId;
        if (userId == null)
            return (null!, Unauthorized(ApiProblemDetails.Unauthorized()));

        var item = await inventoryService.GetInventoryItemAsync(id);
        if (item == null)
            return (null!, NotFound(ApiProblemDetails.NotFound("Item not found.")));

        if (item.UserId != userId)
            return (null!, StatusCode(StatusCodes.Status403Forbidden,
                ApiProblemDetails.Forbidden("You do not have access to this item.")));

        return (item, null);
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
