using MediatR;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Entities;
using QueueLess.Domain.Enums;

namespace QueueLess.Application.Features.Facilities.Commands;

public record CreateFacilityCommand(
    string Name,
    string? Description,
    string Location,
    string OperatingHours
) : IRequest<Guid>;

public class CreateFacilityCommandHandler(IQlDbContext context) : IRequestHandler<CreateFacilityCommand, Guid>
{
    private readonly IQlDbContext _context = context;

    public async Task<Guid> Handle(CreateFacilityCommand request, CancellationToken cancellationToken)
    {
        var facility = new Facility
        {
            Name = request.Name,
            Description = request.Description,
            Location = request.Location,
            OperatingHours = request.OperatingHours,
            Status = QueueStatus.Closed  //Default safe starting state
        };

        _context.Facilities.Add(facility);
        await _context.SaveChangesAsync(cancellationToken);

        return facility.Id;
    }
}