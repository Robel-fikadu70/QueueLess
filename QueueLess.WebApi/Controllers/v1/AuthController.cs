
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QueueLess.Application.DTOs.Auth;
using QueueLess.Application.DTOs.Users;
using QueueLess.Application.Features.Auth.Commands;
using QueueLess.Application.Features.Users.Queries;

namespace QueueLess.WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(ISender sender, IWebHostEnvironment env) : ControllerBase
{
    private readonly ISender _sender = sender;
    private readonly IWebHostEnvironment _env = env;

    [HttpPost("register")]
    [IgnoreAntiforgeryToken] //ignored during registration cause no session exists to forge
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _sender.Send(command);
        //write both tokens to secure httpOnly cookies
        SetTokenCookies(result.AccessToken, result.RefreshToken);
        
        // Return AccessToken in response body
        return NoContent();
    }

    [HttpPost("login")]
    [IgnoreAntiforgeryToken] 
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var result = await _sender.Send(command);

        SetTokenCookies(result.AccessToken, result.RefreshToken);
        
        return NoContent();
    }

    [HttpPost("refresh")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Refresh()
    {
        // 1. Extract the secure refresh token cookie from the request
        if (!Request.Cookies.TryGetValue("X-Refresh-Token", out var refreshToken))
        {
            return Unauthorized("Refresh token is missing.");
        }

        // 2. Perform rotation and theft detection check
        var result = await _sender.Send(new RefreshTokenCommand(refreshToken));
        
        SetTokenCookies(result.AccessToken, result.RefreshToken);

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetCurrentUser()
    {
        var result = await _sender.Send(new GetUserProfileQuery());
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        //Delete both cookies form the browser
        Response.Cookies.Delete("X-Access-Token");
        Response.Cookies.Delete("X-Refresh-Token");
        return NoContent();
    }

    private void SetTokenCookies(string accessToken, string refreshToken)
    {
        var isDevelopment = _env.IsDevelopment();

        // Write the short lived access token cookie
        var accessOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        };
        Response.Cookies.Append("X-Access-Token", accessToken, accessOptions);

        // write the long lived refresh token
        var refreshOptions = new CookieOptions
        {
            HttpOnly = true, // Hide completely from JavaScript (No XSS risks)
            Secure = !isDevelopment, 
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7) // Valid for 7 days
        };

        Response.Cookies.Append("X-Refresh-Token", refreshToken, refreshOptions);
    }
}