
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
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _sender.Send(command);
        SetTokenCookie(result.Token);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginUserCommand command)
    {
        var result = await _sender.Send(command);
        SetTokenCookie(result.Token);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        //Expire the access cookie immediately to clear the user's session
        Response.Cookies.Delete("X-Access-Token");
        return NoContent();
    }

    //secure cookie generation helper
    private void SetTokenCookie(string token)
    {
        var IsDevelopment = _env.IsDevelopment();
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, //strictly hides token from JS XSS attacks
            Secure = !IsDevelopment,
            SameSite = SameSiteMode.Strict, //Strict prevents CSRT cross-origin submissions
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("X-Access-Token", token, cookieOptions);
    }
}