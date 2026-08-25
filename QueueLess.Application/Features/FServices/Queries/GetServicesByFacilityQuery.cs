using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Entities;

namespace QueueLess.Application.Features.FServices.Queries;

public record GetServicesByFacilityQuery(Guid FacilityId) : IRequest<IEnumerable<Service>>;

public class GetServicesByFacilityQueryHandler(IQlDbContext context) : IRequestHandler<GetServicesByFacilityQuery, IEnumerable<Service>>
{
    private readonly IQlDbContext _context = context;

    public async Task<IEnumerable<Service>> Handle(GetServicesByFacilityQuery request, CancellationToken cancellationToken)
    {
        return await _context.Services
            .AsNoTracking()
            .Where(s => s.FacilityId == request.FacilityId)
            .ToListAsync(cancellationToken);
    }
}