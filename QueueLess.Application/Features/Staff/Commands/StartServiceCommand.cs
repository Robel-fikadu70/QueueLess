using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Enums;
using QueueLess.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QueueLess.Application.Features.Staff.Commands;

public record StartServiceCommand(Guid TicketId) : IRequest<Unit>;

public class StartServiceCommandHandler(IQlDbContext context, ICurrentUserService currentUserService, IQueueNotificationService notificationService) : IRequestHandler<StartServiceCommand, Unit>
{
    private readonly IQlDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IQueueNotificationService _notificationService = notificationService;

    public async Task<Unit> Handle(StartServiceCommand request, CancellationToken cancellationToken)
    {
        var staffId = _currentUserService.UserId 
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == request.TicketId && t.ServedByStaffId == staffId, cancellationToken);

        if (ticket == null)
        {
            throw new BusinessRuleException("Ticket assignment not found.");
        }

        if (ticket.State != TicketState.Called && ticket.State != TicketState.CheckedIn)
        {
            throw new BusinessRuleException("Service can only begin on a called ticket.");
        }

        // Transition State: Called -> Serving
        ticket.State = TicketState.Serving;
        ticket.ServedAt = DateTime.UtcNow;
        ticket.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        // Real-Time Push: Notify the customer that counter service has started
        await _notificationService.NotifyTicketStatusChangedAsync(
            ticket.CustomerId, 
            ticket.Id, 
            ticket.TicketNumber, 
            ticket.State.ToString().ToUpper());

        return Unit.Value;
    }
}