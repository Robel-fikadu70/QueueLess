using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.DTOs.Tickets;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Enums;

namespace QueueLess.Application.Features.Tickets.Queries;

public record GetActiveTicketsQuery : IRequest<IEnumerable<TicketHistoryDto>>;

public class GetActiveTicketsQueryHandler(IQlDbContext context, ICurrentUserService currentUser) : IRequestHandler<GetActiveTicketsQuery, IEnumerable<TicketHistoryDto>>
{
    private readonly IQlDbContext _context = context;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<IEnumerable<TicketHistoryDto>> Handle(GetActiveTicketsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var today = DateTime.UtcNow.Date;

        // Fetch only active, un-completed tickets created today for the user
        return await _context.Tickets
            .AsNoTracking()
            .Where(t => t.CustomerId == userId 
                     && (t.State == TicketState.Waiting || t.State == TicketState.Called || t.State == TicketState.Serving)
                     && t.CreatedAt >= today)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketHistoryDto
            {
                TicketId = t.Id,
                TicketNumber = t.TicketNumber,
                ServiceName = t.Service!.Name,
                FacilityName = t.Service.Facility!.Name,
                Status = t.State.ToString().ToUpper(),
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}