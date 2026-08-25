
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueueLess.Application.DTOs.Tickets;
using QueueLess.Application.Features.Tickets.Commands;
using QueueLess.Application.Features.Tickets.Queries;

namespace QueueLess.WebApi.Controllers.v1;

[ApiController]
[Authorize]
[Route("api/v1/tickets")]
public class TicketController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] JoinQueueCommand command)
    {
        var ticketId = await _sender.Send(command);
        return CreatedAtAction(nameof(GetDashboard), new { id = ticketId }, ticketId);
    }

    [HttpPost("{id:guid}/checkin")]
    public async Task<IActionResult> CheckIn(Guid id)
    {
        await _sender.Send(new CheckInTicketCommand(id));
        return NoContent();
    }

    [HttpGet("{id:guid}/dashboard")]
    public async Task<ActionResult<TicketDashboardDto>> GetDashboard(Guid id)
    {
        var result = await _sender.Send(new GetTicketDashboardQuery(id));
        return Ok(result);
    }
    [HttpGet("history")]
    public async Task<ActionResult<System.Collections.Generic.IEnumerable<QueueLess.Domain.Entities.Ticket>>> GetHistory()
    {
        var result = await _sender.Send(new GetTicketHistoryQuery());
        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _sender.Send(new CancelTicketCommand(id));
        return NoContent();
    }
}