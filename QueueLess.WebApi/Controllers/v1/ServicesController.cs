using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueueLess.Application.Features.FServices.Commands;
using QueueLess.Application.Features.FServices.Queries;
using QueueLess.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QueueLess.WebApi.Controllers.v1;

[ApiController]
[Authorize]
[Route("api/v1/services")]
public class ServicesController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpGet("facility/{facilityId:guid}")]
    public async Task<ActionResult<IEnumerable<Service>>> GetByFacility(Guid facilityId)
    {
        var result = await _sender.Send(new GetServicesByFacilityQuery(facilityId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateServiceCommand command)
    {
        var id = await _sender.Send(command);
        return Created(string.Empty, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Mismatched identifier values.");
        }

        await _sender.Send(command);
        return NoContent();
    }
}