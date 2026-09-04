using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Enums;
using QueueLess.Domain.Exceptions;

namespace QueueLess.Application.Features.Facilities.Commands;

public record UpdateFacilityStatusCommand(
    Guid Id,
    QueueStatus Status
) : IRequest<Unit>;

public class UpdateFacilityStatusCommandHandler(
    IQlDbContext context
) : IRequestHandler<UpdateFacilityStatusCommand, Unit>
{
    private readonly IQlDbContext _context = context;

    public async Task<Unit> Handle(
        UpdateFacilityStatusCommand request,
        CancellationToken cancellationToken)
    {
        var facility = await _context.Facilities.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (facility == null)
        {
            throw new BusinessRuleException("Facility not found.");
        }

        facility.Status = request.Status;
        facility.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}