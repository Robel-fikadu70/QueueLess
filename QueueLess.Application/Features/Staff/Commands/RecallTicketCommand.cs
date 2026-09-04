using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QueueLess.Application.Features.Staff.Commands;

public record RecallTicketCommand(Guid TicketId) : IRequest<Unit>;

public class RecallTicketCommandHandler(IQlDbContext context, ICurrentUserService currentUserService, IQueueNotificationService queueNotificationService) : IRequestHandler<RecallTicketCommand, Unit>
{
    private readonly IQlDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IQueueNotificationService _notificationService = queueNotificationService;

    public async Task<Unit> Handle(RecallTicketCommand request, CancellationToken cancellationToken)
    {
        var staffId = _currentUserService.UserId 
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == request.TicketId && t.ServedByStaffId == staffId, cancellationToken);

        if (ticket == null)
        {
            throw new InvalidOperationException("Ticket assignment not found.");
        }

        if (ticket.State != TicketState.NoShow)
        {
            throw new InvalidOperationException("Only skipped no-show tickets can be recalled.");
        }

        // Transition State: NoShow -> Called
        ticket.State = TicketState.Called;
        ticket.CalledAt = DateTime.UtcNow; // Reset call timer
        ticket.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Notify the customer they are being recalled
        await _notificationService.NotifyTicketStatusChangedAsync(
            ticket.CustomerId, 
            ticket.Id, 
            ticket.TicketNumber, 
            ticket.State.ToString().ToUpper());

        // REAL-TIME BROADCAST: Notify staff dashboard to refresh lists
        await _notificationService.NotifyQueuePositionChangedAsync(ticket.ServiceId);
 

        return Unit.Value;
    }
}