using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QueueLess.Application.Features.Staff.Commands;

public record CallNextTicketCommand(Guid ServiceId) : IRequest<Guid?>;

public class CallNextTicketCommandHandler(IQlDbContext context, ICurrentUserService currentUserService) : IRequestHandler<CallNextTicketCommand, Guid?>
{
    private readonly IQlDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Guid?> Handle(CallNextTicketCommand request, CancellationToken cancellationToken)
    {
        var staffId = _currentUserService.UserId 
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        // Fetch the earliest waiting ticket today for this service queue
        var today = DateTime.UtcNow.Date;
        var nextTicket = await _context.Tickets
            .Where(t => t.ServiceId == request.ServiceId 
                     && t.State == TicketState.Waiting 
                     && t.CreatedAt >= today)
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

        return nextTicket.Id;
    }
}