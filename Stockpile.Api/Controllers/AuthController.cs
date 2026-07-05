using Microsoft.AspNetCore.Mvc;
using Stockpile.Api.Contracts.Requests;
using Stockpile.Api.Contracts.Response;
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest? user)
    {
        if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
            return BadRequest(ApiProblemDetails.BadRequest("Invalid user credentials."));
        
        if (user.Password != "password") 
            return Unauthorized(ApiProblemDetails.Unauthorized("Invalid username or password."));
        
        var userProfile = await userProfileService.GetUserProfile(user.Username);
        var token = tokenService.GenerateToken(userProfile);
        return Ok(token);
    }
}