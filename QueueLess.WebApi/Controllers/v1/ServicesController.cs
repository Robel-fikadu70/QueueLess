using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueueLess.Application.DTOs.F_Services;
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceRequest request)
    {
        var command = new UpdateServiceCommand(
            id,
            request.Name,
            request.Description,
            request.EstimatedDurationMinutes
        );

        await _sender.Send(command);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] bool isActive)
    {
        await _sender.Send(new UpdateServiceStatusCommand(id, isActive));

        return NoContent();
    }
}