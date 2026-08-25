using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Entities;
using QueueLess.Domain.Enums;

namespace QueueLess.Application.Features.Tickets.Queries;

public record GetTicketHistoryQuery : IRequest<IEnumerable<Ticket>>;

public class GetTicketHistoryQueryHandler(IQlDbContext context, ICurrentUserService currentUser) : IRequestHandler<GetTicketHistoryQuery, IEnumerable<Ticket>>
{
    private readonly IQlDbContext _context = context;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<IEnumerable<Ticket>> Handle(GetTicketHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        return await _context.Tickets
            .AsNoTracking()
            .Include(t => t.Service)
            .ThenInclude(s => s!.Facility)
            .Where(t => t.CustomerId == userId 
                     && (t.State == TicketState.Completed || t.State == TicketState.Cancelled || t.State == TicketState.NoShow))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}