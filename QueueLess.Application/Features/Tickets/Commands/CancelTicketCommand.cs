using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Enums;

namespace QueueLess.Application.Features.Tickets.Commands;
public record CancelTicketCommand(Guid TicketId): IRequest<Unit>;

public class CancelTicketCommandHandler(IQlDbContext qlDbContext, ICurrentUserService currentUserService, IQueueNotificationService notificationService) : IRequestHandler<CancelTicketCommand, Unit>
{
    private readonly IQlDbContext _context = qlDbContext;
    private readonly ICurrentUserService _currentUser = currentUserService;
    private readonly IQueueNotificationService _notificationService = notificationService;

    public async Task<Unit> Handle(CancelTicketCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == request.TicketId && t.CustomerId == userId, cancellationToken);

        if(ticket == null)
        {
            throw new InvalidOperationException("Ticket not found.");
        }
        if(ticket.State != TicketState.Waiting && ticket.State != TicketState.Called)
        {
            throw new InvalidOperationException("You can only cancle waiting or called tickets.");
        }

        ticket.State = TicketState.Cancelled;
        ticket.LastModifiedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyQueuePositionChangedAsync(ticket.ServiceId);

        return Unit.Value;
    }
}