using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueueLess.Application.DTOs.Admin;
using QueueLess.Application.DTOs.Staff;
using QueueLess.Application.Features.Admin.Queries;
using QueueLess.Application.Features.Staff.Commands;
using QueueLess.Application.Interfaces;


namespace QueueLess.WebApi.Controllers.v1;

[ApiController]
[Authorize(Roles = "Admin")] // Strictly limits these administrative requests to Admins
[Route("api/v1/admin")]
public class AdminController(ISender sender, IIdentityService identityService) : ControllerBase
{
    private readonly ISender _sender = sender;
    private readonly IIdentityService _identityService = identityService;

    [HttpGet("dashboard-stats")]
    public async Task<ActionResult<AdminDashboardStatsDto>> GetStats()
    {
        var result = await _sender.Send(new GetAdminDashboardStatsQuery());
        return Ok(result);
    }

    [HttpGet("staff")]
    public async Task<ActionResult<IEnumerable<StaffMemberDto>>> GetStaff()
    {
        var result = await _identityService.GetStaffUsersAsync();
        return Ok(result);
    }

    [HttpPost("staff/register")]
    public async Task<IActionResult> RegisterStaff([FromBody] RegisterStaffMemberCommand command)
    {
        var staffId = await _sender.Send(command);
        return Created(string.Empty, new { StaffId = staffId });
    }

    [HttpPut("staff/assignment")]
    public async Task<IActionResult> AssignStaff([FromBody] AssignStaffToServiceCommand command)
    {
        await _sender.Send(command);
        return NoContent();
    }
}