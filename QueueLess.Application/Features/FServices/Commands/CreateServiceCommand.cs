using MediatR;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Entities;


namespace QueueLess.Application.Features.FServices.Commands;

public record CreateServiceCommand(
    Guid FacilityId,
    string Name,
    string? Description,
    int EstimatedDurationMinutes
) : IRequest<Guid>;

public class CreateServiceCommandHandler(IQlDbContext context) : IRequestHandler<CreateServiceCommand, Guid>
{
    private readonly IQlDbContext _context = context;

    public async Task<Guid> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = new Service
        {
            FacilityId = request.FacilityId,
            Name = request.Name,
            Description = request.Description,
            EstimatedDurationMinutes = request.EstimatedDurationMinutes,
            IsActive = true
        };

        _context.Services.Add(service);
        await _context.SaveChangesAsync(cancellationToken);

        return service.Id;
    }
}