using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueueLess.Application.DTOs.Users;
using QueueLess.Application.Features.Users.Commands;
using QueueLess.Application.Features.Users.Queries;
using System.Threading.Tasks;

namespace QueueLess.WebApi.Controllers.v1;

[ApiController]
[Authorize] // Requires users to be authenticated
[Route("api/v1/users")]
public class UsersController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var result = await _sender.Send(new GetUserProfileQuery());
        return Ok(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileCommand command)
    {
        await _sender.Send(command);
        return NoContent();
    }
}