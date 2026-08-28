using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QueueLess.Application.Interfaces;
using QueueLess.Domain.Entities;
using QueueLess.Infrastructure.Identity;

namespace QueueLess.Infrastructure.Persistence;

public class QlDbContext(DbContextOptions<QlDbContext> options) : IdentityDbContext<ApplicationUser>(options), IQlDbContext
{
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<StaffAssignment> StaffAssignments => Set<StaffAssignment>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}