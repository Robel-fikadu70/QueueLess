using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.DTOs.Staff;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QueueLess.Application.Features.Staff.Queries;

public record GetStaffDashboardQuery(Guid ServiceId) : IRequest<StaffDashboardDto>;

public class GetStaffDashboardQueryHandler(IQlDbContext context, ICurrentUserService currentUserService) : IRequestHandler<GetStaffDashboardQuery, StaffDashboardDto>
{
    private readonly IQlDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<StaffDashboardDto> Handle(GetStaffDashboardQuery request, CancellationToken cancellationToken)
    {
        var staffId = _currentUserService.UserId 
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var today = DateTime.UtcNow.Date;

        // 1. Fetch actively called or serving ticket assigned to this staff member
        var activeTicket = await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.ServiceId == request.ServiceId 
                                   && t.ServedByStaffId == staffId 
                                   && (t.State == TicketState.Called || t.State == TicketState.Serving || t.State == TicketState.CheckedIn) 
                                   && t.CreatedAt >= today, cancellationToken);

        // 2. Fetch all tickets currently waiting in line
        var waitingList = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.ServiceId == request.ServiceId && t.ServedByStaffId == null
                     && (t.State == TicketState.Waiting || t.State == TicketState.CheckedIn )
                     && t.CreatedAt >= today)
            .OrderBy(t => t.SequenceNumber)
            .ToListAsync(cancellationToken);

        // 3. Fetch recent completions or skips processed today
        var recentList = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.ServiceId == request.ServiceId 
                     && t.ServedByStaffId == staffId 
                     && (t.State == TicketState.Completed || t.State == TicketState.NoShow) 
                     && t.CreatedAt >= today)
            .OrderByDescending(t => t.LastModifiedAt)
            .Take(10) // Limit count to reduce payload sizes
            .ToListAsync(cancellationToken);

        return new StaffDashboardDto
        {
            CurrentlyServing = activeTicket,
            WaitingList = waitingList,
            RecentActivity = recentList
        };
    }
}