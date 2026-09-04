using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Enums;
using QueueLess.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QueueLess.Application.Features.Tickets.Commands;

public record CheckInTicketCommand(Guid TicketId) : IRequest<Unit>;

public class CheckInTicketCommandHandler(IQlDbContext context, ICurrentUserService currentUserService) : IRequestHandler<CheckInTicketCommand, Unit>
{
    private readonly IQlDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Unit> Handle(CheckInTicketCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentUserService.UserId 
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == request.TicketId && t.CustomerId == customerId, cancellationToken);

        if (ticket == null)
        {
            throw new BusinessRuleException("Ticket record not found.");
        }

        if (ticket.State != TicketState.Waiting && ticket.State != TicketState.Called)
        {
            throw new BusinessRuleException("You can only check in for an active waiting or called ticket.");
        }

        // Record arrival timestamp
        ticket.State = TicketState.CheckedIn;
        ticket.CheckedInAt = DateTime.UtcNow;
        ticket.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}