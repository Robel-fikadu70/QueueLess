using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Exceptions;

namespace QueueLess.Application.Features.FServices.Commands;

public record UpdateServiceStatusCommand(
    Guid Id,
    bool IsActive 
) : IRequest<Unit>;

public class UpdateServiceStatusCommandHandler(IQlDbContext context): IRequestHandler<UpdateServiceStatusCommand, Unit>
{
    private readonly IQlDbContext _context = context;

    public async Task<Unit> Handle(UpdateServiceStatusCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (service == null)
        {
            throw new BusinessRuleException("Service not found.");
        }

        service.IsActive = request.IsActive;
        service.LastModifiedAt= DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

}