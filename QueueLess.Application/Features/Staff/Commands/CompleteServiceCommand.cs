using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QueueLess.Application.Features.Staff.Commands;

public record CompleteServiceCommand(Guid TicketId) : IRequest<Unit>;

public class CompleteServiceCommandHandler(IQlDbContext context, ICurrentUserService currentUserService) : IRequestHandler<CompleteServiceCommand, Unit>
{
    private readonly IQlDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Unit> Handle(CompleteServiceCommand request, CancellationToken cancellationToken)
    {
        var staffId = _currentUserService.UserId 
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == request.TicketId && t.ServedByStaffId == staffId, cancellationToken);

        if (ticket == null)
        {
            throw new InvalidOperationException("Ticket assignment not found.");
        }

        if (ticket.State != TicketState.Serving)
        {
            throw new InvalidOperationException("Service can only be completed on an actively serving ticket.");
        }

        // Transition State: Serving -> Completed
        ticket.State = TicketState.Completed;
        ticket.CompletedAt = DateTime.UtcNow;
        ticket.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}