using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueueLess.Application.Features.Facilities.Commands;
using QueueLess.Application.Features.Facilities.Queries;
using QueueLess.Domain.Entities;

namespace QueueLess.WebApi.Controllers.v1;

[ApiController]
[Authorize] //requires users to be authenticated to read resources
[Route("api/v1/facilities")]
public class FacilitiesController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Facility>>> GetAllActive()
    {
        var result = await _sender.Send(new GetActiveFacilitiesQuery());
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateFacilityCommand command)
    {
        var id = await _sender.Send(command);
        return CreatedAtAction(nameof(GetAllActive), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFacilityCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Mismatched identifier values.");
        }

        await _sender.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteFacilityCommand(id));
        return NoContent();
    }
}
