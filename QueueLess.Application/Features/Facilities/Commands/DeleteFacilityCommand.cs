using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Exceptions;

namespace QueueLess.Application.Features.Facilities.Commands;

public record DeleteFacilityCommand(Guid Id) : IRequest<Unit>;

public class DeleteFacilityCommandHandler(IQlDbContext context) : IRequestHandler<DeleteFacilityCommand, Unit>
{
    private readonly IQlDbContext _context = context;

    public async Task<Unit> Handle(DeleteFacilityCommand request, CancellationToken cancellationToken)
    {
        var facility = await _context.Facilities.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if(facility == null)
        {
            throw new BusinessRuleException("Facility not found.");
        }

        //Soft delete
        facility.IsDeleted = true;
        facility.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}