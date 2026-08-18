using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QueueLess.Application.Features.Staff.Commands;

public record SkipNoShowCommand(Guid TicketId) : IRequest<Unit>;

public class SkipNoShowCommandHandler(IQlDbContext context, ICurrentUserService currentUserService) : IRequestHandler<SkipNoShowCommand, Unit>
{
    private readonly IQlDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Unit> Handle(SkipNoShowCommand request, CancellationToken cancellationToken)
    {
        var staffId = _currentUserService.UserId 
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == request.TicketId && t.ServedByStaffId == staffId, cancellationToken);

        if (ticket == null)
        {
            throw new InvalidOperationException("Ticket assignment not found.");
        }

        if (ticket.State != TicketState.Called)
        {
            throw new InvalidOperationException("Tickets can only be skipped from the called state.");
        }

        // Transition State: Called -> NoShow
        ticket.State = TicketState.NoShow;
        ticket.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}