using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Enums;

namespace QueueLess.Application.Features.Staff.Commands;

public record CallNextTicketCommand(Guid ServiceId) : IRequest<Guid?>;

public class CallNextTicketCommandHandler(
    IQlDbContext context,
    ICurrentUserService currentUserService,
    ITicketExpiryQueue expiryQueue,
    IQueueNotificationService notificationService
) : IRequestHandler<CallNextTicketCommand, Guid?>
{
    private readonly IQlDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ITicketExpiryQueue _expiryQueue = expiryQueue;
    private readonly IQueueNotificationService _notificationService = notificationService;

    public async Task<Guid?> Handle(
        CallNextTicketCommand request,
        CancellationToken cancellationToken
    )
    {
        var staffId =
            _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        // Fetch the earliest waiting ticket today for this service queue
        var today = DateTime.UtcNow.Date;
        var nextTicket = await _context
            .Tickets.Where(t =>
                t.ServiceId == request.ServiceId
                && t.State == TicketState.Waiting
                && t.CreatedAt >= today
            )
            .OrderBy(t => t.SequenceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (nextTicket == null)
        {
            return null; // Queue is currently empty
        }

        // Transition State: Waiting -> Called
        nextTicket.State = TicketState.Called;
        nextTicket.CalledAt = DateTime.UtcNow;
        nextTicket.ServedByStaffId = staffId;
        nextTicket.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyTicketStatusChangedAsync(
            nextTicket.CustomerId,
            nextTicket.Id,
            nextTicket.TicketNumber,
            nextTicket.State.ToString().ToUpper()
        );

        await _notificationService.NotifyQueuePositionChangedAsync(nextTicket.ServiceId);

        //Dynamic Expiry Queue Task Creation: Configured with a default 5 minute grace period
        var gracePeriodMinuets = 5;
        var expirationTimestamp = DateTime.UtcNow.AddMinutes(gracePeriodMinuets);

        await _expiryQueue.QueueExpiryCheckAsync(
            new Common.Models.TicketExpiryTask(nextTicket.Id, expirationTimestamp),
            cancellationToken
        );

        return nextTicket.Id;
    }
}
