using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.DTOs.Admin;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Enums;

namespace QueueLess.Application.Features.Admin.Queries;

public record GetAdminDashboardStatsQuery : IRequest<AdminDashboardStatsDto>;

public class GetAdminDashboardStatsQueryHandler(IQlDbContext context, IIdentityService identityService) : IRequestHandler<GetAdminDashboardStatsQuery, AdminDashboardStatsDto>
{
    private readonly IQlDbContext _context = context;
    private readonly IIdentityService _identityService = identityService;

    public async Task<AdminDashboardStatsDto> Handle(GetAdminDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var activeFacilities = await _context.Facilities.CountAsync(cancellationToken);
        var activeServices = await _context.Services.CountAsync(s => s.IsActive, cancellationToken);
        
        var staffUsers = await _identityService.GetStaffUsersAsync();
        var activeStaffCount = staffUsers.Count();

        var today = DateTime.UtcNow.Date;

        var waitingCount = await _context.Tickets
            .CountAsync(t => t.State == TicketState.Waiting && t.CreatedAt >= today, cancellationToken);

        var servedTodayCount = await _context.Tickets
            .CountAsync(t => t.State == TicketState.Completed && t.CreatedAt >= today, cancellationToken);

        return new AdminDashboardStatsDto
        {
            ActiveFacilities = activeFacilities,
            ActiveServices = activeServices,
            ActiveStaff = activeStaffCount,
            CustomersWaiting = waitingCount,
            CustomersServedToday = servedTodayCount
        };
    }
}