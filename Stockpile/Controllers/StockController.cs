using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockpile.Contracts.Mappers;
using Stockpile.Contracts.Requests;
using Stockpile.Contracts.Response;
using Stockpile.DAL.Models;
using Stockpile.Services;

namespace Stockpile.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StockController(IInventoryService inventoryService, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<InventoryItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<InventoryItem>>> GetStock()
    {
        var username = currentUserService.Username;
        if(username == null)
            return Unauthorized();
        
        var results = await inventoryService.GetInventoryItemsByUserAsync(username);
        var inventoryItems = results.Select(InventoryItemMapper.Map);
        return Ok(inventoryItems);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(List<InventoryItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<InventoryItem>>> GetStock(string id)
    {
        var result = await inventoryService.GetInventoryItemAsync(id);
        if(result == null)
            return NotFound("Item not found");
        
        if(result.Username != currentUserService.Username)
            return Unauthorized("You do not have access to this item");
        
        return Ok(result.Map());
    }

    [HttpPost]
    [ProducesResponseType(typeof(InventoryItemResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<InventoryItemResponse>> AddStock(InventoryItemCreateRequest request)
    {
        var userId = currentUserService.UserId;
        var username = currentUserService.Username;
        if(username == null || userId == null)
            return Unauthorized();
        
        var item = request.Map(userId, username);
        await inventoryService.AddInventoryItemAsync(item);
        return CreatedAtAction(nameof(GetStock), new { id = item.Id }, item.Map());
    }
    
}