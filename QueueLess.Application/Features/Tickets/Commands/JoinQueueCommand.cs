using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Entities;
using QueueLess.Domain.Enums;
using QueueLess.Domain.Exceptions;

namespace QueueLess.Application.Features.Tickets.Commands;

public record JoinQueueCommand(Guid ServiceId) : IRequest<Guid>;

public class JoinQueueCommandHandler(IQlDbContext context, ICurrentUserService currentUserService) : IRequestHandler<JoinQueueCommand, Guid>
{
    private readonly IQlDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Guid> Handle(JoinQueueCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        // Fetch service and ensure the associated facility is active
        var service = await _context.Services
            .Include(s => s.Facility)
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (service == null || !service.IsActive)
        {
            throw new BusinessRuleException("The requested service queue is inactive or does not exist.");
        }

        if (service.Facility!.Status != QueueStatus.Open)
        {
            throw new BusinessRuleException("The facility hosting this queue is currently closed or paused.");
        }

        // Check if the user is already waiting in this specific service queue
        var existingTicket = await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.ServiceId == request.ServiceId
                                   && t.CustomerId == customerId
                                   && (t.State == TicketState.Waiting || t.State == TicketState.CheckedIn || t.State == TicketState.Called || t.State == TicketState.Serving),
                                 cancellationToken);

        if (existingTicket != null)
        {
            throw new BusinessRuleException("You are already active in this queue.");
        }

        // Calculate sequence number resetting daily per service queue
        var today = DateTime.UtcNow.Date;
        var dailyTicketCount = await _context.Tickets
            .IgnoreQueryFilters() // Must include soft-deleted/cancelled history for accurate numbering
            .CountAsync(t => t.ServiceId == request.ServiceId && t.CreatedAt >= today, cancellationToken);

        var nextSequence = dailyTicketCount + 1;

        // Generate Ticket Prefix (e.g., Laboratory -> "LAB-001")
        var prefix = service.Name.Length >= 3
            ? service.Name[..3].ToUpper()
            : service.Name.ToUpper();

        var ticketNumber = $"{prefix}-{nextSequence}";

        var ticket = new Ticket
        {
            ServiceId = request.ServiceId,
            CustomerId = customerId,
            SequenceNumber = nextSequence,
            TicketNumber = ticketNumber,
            State = TicketState.Waiting
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync(cancellationToken);

        return ticket.Id;
    }
}