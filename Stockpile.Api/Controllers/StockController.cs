using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockpile.Api.Contracts.Mappers;
using Stockpile.Api.Contracts.Requests;
using Stockpile.Api.Contracts.Response;
using Stockpile.Api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Stockpile.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StockController(IInventoryService inventoryService, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(OperationId ="GetStock")]
    [ProducesResponseType(typeof(List<InventoryItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<InventoryItemResponse>>> GetStock()
    {
        var username = currentUserService.Username;
        if(username == null)
            return Unauthorized();
        
        var results = await inventoryService.GetInventoryItemsByUserAsync(username);
        var inventoryItems = results.Select(InventoryItemMapper.MapFrom);
        return Ok(inventoryItems);
    }
    
    [HttpGet("{id}")]
    [SwaggerOperation(OperationId ="GetStockById")]
    [ProducesResponseType(typeof(InventoryItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryItemResponse>> GetStock(string id)
    {
        var result = await inventoryService.GetInventoryItemAsync(id);
        if(result == null)
            return NotFound("Item not found");
        
        if(result.Username != currentUserService.Username)
            return Unauthorized("You do not have access to this item");
        
        return Ok(result.MapFrom());
    }

    [HttpPost]
    [SwaggerOperation(OperationId ="AddStock")]
    [ProducesResponseType(typeof(InventoryItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<InventoryItemResponse>> AddStock(InventoryItemRequest request)
    {
        var userId = currentUserService.UserId;
        var username = currentUserService.Username;
        if(username == null || userId == null)
            return Unauthorized();
        
        var item = request.MapTo(userId, username);
        await inventoryService.AddInventoryItemAsync(item);
        return CreatedAtAction(nameof(GetStock), new { id = item.Id }, item.MapFrom());
    }
    
    [HttpPut("{id}")]
    [SwaggerOperation(OperationId ="UpdateStock")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(InventoryItemResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventoryItemResponse>> UpdateStock(string id, InventoryItemRequest request)
    {
        var userId = currentUserService.UserId;
        var username = currentUserService.Username;
        if(username == null || userId == null)
            return Unauthorized();
        
        var item = request.MapTo(id, userId, username);
        
        await inventoryService.UpdateInventoryItemAsync(item);
        return Ok(item.MapFrom());
    }
    
    [HttpDelete("{id}")]
    [SwaggerOperation(OperationId ="DeleteStock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStock(string id)
    {
        var result = await inventoryService.GetInventoryItemAsync(id);
        if(result == null)
            return NotFound("Item not found");
        
        if(result.Username != currentUserService.Username)
            return Unauthorized("You do not have access to this item");
        
        await inventoryService.DeleteInventoryItemAsync(id);
        return NoContent();
    }
}