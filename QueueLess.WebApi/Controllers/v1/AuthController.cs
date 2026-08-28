
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QueueLess.Application.DTOs.Auth;
using QueueLess.Application.Features.Auth.Commands;

namespace QueueLess.WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(ISender sender, IWebHostEnvironment env) : ControllerBase
{
    private readonly ISender _sender = sender;
    private readonly IWebHostEnvironment _env = env;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _sender.Send(command);
        SetRefreshTokenCookie(result.RefreshToken);
        
        // Return AccessToken in response body
        return Ok(new { AccessToken = result.AccessToken });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var result = await _sender.Send(command);
        SetRefreshTokenCookie(result.RefreshToken);
        
        return Ok(new { AccessToken = result.AccessToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        // 1. Extract the secure refresh token cookie from the request
        if (!Request.Cookies.TryGetValue("X-Refresh-Token", out var refreshToken))
        {
            return BadRequest("Refresh token is missing.");
        }

        // 2. Perform rotation and theft detection check
        var result = await _sender.Send(new RefreshTokenCommand(refreshToken));
        
        // 3. Append the newly rotated refresh token cookie
        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(new { AccessToken = result.AccessToken });
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("X-Refresh-Token");
        return NoContent();
    }

    private void SetRefreshTokenCookie(string token)
    {
        var isDevelopment = _env.IsDevelopment();

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // Hide completely from JavaScript (No XSS risks)
            Secure = !isDevelopment, 
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7) // Valid for 7 days
        };

        Response.Cookies.Append("X-Refresh-Token", token, cookieOptions);
    }
}