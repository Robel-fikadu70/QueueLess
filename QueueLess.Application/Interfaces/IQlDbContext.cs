using Microsoft.EntityFrameworkCore;
using QueueLess.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
namespace QueueLess.Application.Interfaces;

public interface IQlDbContext
{
    DbSet<Facility> Facilities {get ;}
    DbSet<Service> Services {get ;}
    DbSet<Ticket> Tickets {get ;}
    DbSet<StaffAssignment> StaffAssignments {get ;}

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

}