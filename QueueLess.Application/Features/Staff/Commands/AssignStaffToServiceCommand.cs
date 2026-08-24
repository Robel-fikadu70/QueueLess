using MediatR;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QueueLess.Application.Features.Staff.Commands;

public record AssignStaffToServiceCommand(string StaffId, Guid ServiceId, int CounterNumber) : IRequest<Unit>;

public class AssignStaffToServiceCommandHandler(IQlDbContext context) : IRequestHandler<AssignStaffToServiceCommand, Unit>
{
    private readonly IQlDbContext _context = context;

    public async Task<Unit> Handle(AssignStaffToServiceCommand request, CancellationToken cancellationToken)
    {
        // Deactivate previous active assignments for this specific Staff member
        var activeAssignments = await _context.StaffAssignments
            .Where(sa => sa.StaffId == request.StaffId && sa.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var oldAssignment in activeAssignments)
        {
            oldAssignment.IsActive = false;
            oldAssignment.LastModifiedAt = DateTime.UtcNow;
        }

        // Establish the new service queue assignment
        var assignment = new StaffAssignment
        {
            StaffId = request.StaffId,
            ServiceId = request.ServiceId,
            CounterNumber = request.CounterNumber,
            IsActive = true
        };

        _context.StaffAssignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}