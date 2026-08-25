using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Enums;

namespace QueueLess.Application.Features.Facilities.Commands;

public record UpdateFacilityCommand(
    Guid Id,
    string Name,
    string? Description,
    string Location,
    string OperatingHours,
    QueueStatus Status
) : IRequest<Unit>;

public class UpdateFacilityCommandHandler(IQlDbContext context) : IRequestHandler<UpdateFacilityCommand, Unit>
{
    private readonly IQlDbContext _context = context;

    public async Task<Unit> Handle(UpdateFacilityCommand request, CancellationToken cancellationToken)
    {
        var facility = await _context.Facilities.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (facility == null)
        {
            throw new InvalidOperationException("Facility not found.");
        }

        facility.Name = request.Name;
        facility.Description = request.Description;
        facility.Location = request.Location;
        facility.OperatingHours = request.OperatingHours;
        facility.Status = request.Status;
        facility.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}