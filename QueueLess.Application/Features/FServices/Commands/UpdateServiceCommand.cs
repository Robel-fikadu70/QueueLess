using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QueueLess.Application.Features.FServices.Commands;

public record UpdateServiceCommand(
    Guid Id,
    string Name,
    string? Description,
    int EstimatedDurationMinutes,
    bool IsActive
) : IRequest<Unit>;

public class UpdateServiceCommandHandler(IQlDbContext context) : IRequestHandler<UpdateServiceCommand, Unit>
{
    private readonly IQlDbContext _context = context;

    public async Task<Unit> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service == null)
        {
            throw new InvalidOperationException("Facility Service not found.");
        }

        service.Name = request.Name;
        service.Description = request.Description;
        service.EstimatedDurationMinutes = request.EstimatedDurationMinutes;
        service.IsActive = request.IsActive;
        service.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}