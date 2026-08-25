using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QueueLess.Domain.Entities;

namespace QueueLess.Infrastructure.Persistence.Configurations;

public class StaffAssignmentConfiguration : IEntityTypeConfiguration<StaffAssignment>
{
    public void Configure(EntityTypeBuilder<StaffAssignment> builder)
    {
        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.StaffId)
            .IsRequired();

        builder.HasQueryFilter(sa => !sa.IsDeleted && !sa.Service!.IsDeleted);

        // Service has many StaffAssignments.
        builder.HasOne(sa => sa.Service)
            .WithMany(s => s.StaffAssignments)
            .HasForeignKey(sa => sa.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}