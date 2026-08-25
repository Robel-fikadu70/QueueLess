using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Entities;

namespace QueueLess.Application.Features.Facilities.Queries;

public record GetActiveFacilitiesQuery : IRequest<IEnumerable<Facility>>;

public class GetActiveFacilitiesQueryHandler(IQlDbContext context) : IRequestHandler<GetActiveFacilitiesQuery, IEnumerable<Facility>>
{
    private readonly IQlDbContext _context = context;

    public async Task<IEnumerable<Facility>> Handle(GetActiveFacilitiesQuery request, CancellationToken cancellationToken)
        {
            // Read-only pipeline using AsNoTracking for optimal execution speed
            return await _context.Facilities
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
}