using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.DTOs.Tickets;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Enums;
using QueueLess.Domain.Exceptions;

namespace QueueLess.Application.Features.Tickets.Queries;

public record GetTicketDashboardQuery(Guid TicketId) : IRequest<TicketDashboardDto>;

public class GetTicketDashboardQueryHandler(IQlDbContext context, ICurrentUserService currentUserService) : IRequestHandler<GetTicketDashboardQuery, TicketDashboardDto>
{
    private readonly IQlDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<TicketDashboardDto> Handle(GetTicketDashboardQuery request, CancellationToken cancellationToken)
    {
        var customerId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");

        //Fetch user ticket with its related service and facility
        var ticket = await _context.Tickets
                            .Include(t => t.Service)
                            .ThenInclude(s => s!.Facility)
                            .FirstOrDefaultAsync(t => t.Id == request.TicketId && t.CustomerId == customerId, cancellationToken);

        if (ticket == null)
        {
            throw new BusinessRuleException("Active ticket not found.");
        }

        var today = DateTime.UtcNow.Date;

        // 1. Calculate ppl ahead(Ticket created today with an earlier sequence number still in the waiting state)
        var peopleAhead = await _context.Tickets.CountAsync(t => t.ServiceId == ticket.ServiceId
                                                            && (t.State == TicketState.Waiting || t.State == TicketState.CheckedIn )
                                                            && t.SequenceNumber < ticket.SequenceNumber
                                                            && t.CreatedAt >= today, cancellationToken);

        //2. Fetch the Ticket currently being served at the counter
        var currentlyServingTicket = await _context.Tickets.AsNoTracking().Where(t => t.ServiceId == ticket.ServiceId
                                                                                    && (t.State == TicketState.Serving || t.State == TicketState.Called)
                                                                                    && t.CreatedAt >= today).OrderByDescending(t => t.SequenceNumber).FirstOrDefaultAsync(cancellationToken);
        var currentTicketNumber = currentlyServingTicket?.TicketNumber ?? "None";

        //3. count active staff members serving this service queue
        var activeCounters = await _context.StaffAssignments.CountAsync(sa => sa.ServiceId == ticket.ServiceId && sa.IsActive, cancellationToken);

        //Establish safe fallback of at least 1 counter to prevent division by zero
        var counterCount = activeCounters > 0 ? activeCounters : 1;

        //4. calculate dynamic waiting time range
        var averageDuration = ticket.Service!.EstimatedDurationMinutes;
        var totalEstimatedMinuets = (peopleAhead * averageDuration) / counterCount;

        string waitingRange;
        if (ticket.State == TicketState.Serving)
        {
            waitingRange = "Your turn is being served";
        }
        else if (ticket.State == TicketState.Called)
        {
            waitingRange = "Please proceed to the counter";
        }
        else if (peopleAhead == 0)
        {
            waitingRange = "You are next in line (0-5 minutes)";
        }
        else
        {
            // create a realistic estimation range (80% to 120% of estimated time)
            var lowerBound = (int)Math.Max(5, totalEstimatedMinuets * 0.8);
            var upperBound = (int)(totalEstimatedMinuets * 1.2);
            waitingRange = $"{lowerBound} - {upperBound} minutes";
        }

        return new TicketDashboardDto
        {
            FacilityName = ticket.Service.Facility!.Name,
            ServiceName = ticket.Service.Name,
            TicketNumber = ticket.TicketNumber,
            PeopleAhead = peopleAhead,
            CurrentTicketBeingServed = currentTicketNumber,
            EstimatedWaitRange = waitingRange,
            QueueStatus = ticket.Service.Facility.Status.ToString().ToUpper(),
            CheckInStatus = ticket.State 
        };

    }
}