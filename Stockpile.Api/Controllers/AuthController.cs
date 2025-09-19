using Microsoft.AspNetCore.Mvc;
using Stockpile.Api.Contracts.Requests;
using Stockpile.Api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Stockpile.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ITokenService tokenService, IUserProfileService userProfileService) : ControllerBase
{
    [HttpPost("login")]
    [SwaggerOperation(OperationId ="Login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest? user)
    {
        if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
            return BadRequest("Invalid user credentials.");
        
        if (user.Username != "DemoUser1" || user.Password != "password") 
            return Unauthorized();
        
        var userProfile = await userProfileService.GetUserProfile(user.Username);
        var token = tokenService.GenerateToken(userProfile);
        return Ok(token);
    }
}