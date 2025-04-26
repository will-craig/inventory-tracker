using Microsoft.AspNetCore.Mvc;
using Stockpile.Contracts.Requests;
using Stockpile.Exceptions;
using Stockpile.Services;

namespace Stockpile.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ITokenService tokenService, IUserProfileService userProfileService) : ControllerBase
{
    [HttpPost("login")]
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