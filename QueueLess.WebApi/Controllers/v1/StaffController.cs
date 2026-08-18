using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueueLess.Application.DTOs.Staff;
using QueueLess.Application.Features.Staff.Commands;
using QueueLess.Application.Features.Staff.Queries;
using System;
using System.Threading.Tasks;

namespace QueueLess.WebApi.Controllers.v1;

[ApiController]
[Authorize(Roles = "Staff")] // Restricts execution purely to users assigned to the Staff role
[Route("api/v1/staff")]
public class StaffController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpGet("dashboard/{serviceId:guid}")]
    public async Task<ActionResult<StaffDashboardDto>> GetDashboard(Guid serviceId)
    {
        var result = await _sender.Send(new GetStaffDashboardQuery(serviceId));
        return Ok(result);
    }

    [HttpPost("call-next")]
    public async Task<IActionResult> CallNext([FromBody] CallNextTicketCommand command)
    {
        var ticketId = await _sender.Send(command);
        if (ticketId == null)
        {
            return NoContent(); // No active waiting customers
        }
        return Ok(new { TicketId = ticketId });
    }

    [HttpPost("start-service/{ticketId:guid}")]
    public async Task<IActionResult> StartService(Guid ticketId)
    {
        await _sender.Send(new StartServiceCommand(ticketId));
        return NoContent();
    }

    [HttpPost("complete-service/{ticketId:guid}")]
    public async Task<IActionResult> CompleteService(Guid ticketId)
    {
        await _sender.Send(new CompleteServiceCommand(ticketId));
        return NoContent();
    }

    [HttpPost("skip-noshow/{ticketId:guid}")]
    public async Task<IActionResult> SkipNoShow(Guid ticketId)
    {
        await _sender.Send(new SkipNoShowCommand(ticketId));
        return NoContent();
    }

    [HttpPost("recall/{ticketId:guid}")]
    public async Task<IActionResult> Recall(Guid ticketId)
    {
        await _sender.Send(new RecallTicketCommand(ticketId));
        return NoContent();
    }
}