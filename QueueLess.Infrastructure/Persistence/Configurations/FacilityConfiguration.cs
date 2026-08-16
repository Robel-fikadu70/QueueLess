
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using QueueLess.Domain.Entities;

namespace QueueLess.Infrastructure.Persistence.Configurations;

public class FacilityConfiguration : IEntityTypeConfiguration<Facility>
{
    public void Configure(EntityTypeBuilder<Facility> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name).IsRequired().HasMaxLength(150);
        builder.Property(f => f.OperatingHours).IsRequired().HasMaxLength(50);
        //Map enum to string in Postgres for readability
        builder.Property(f => f.Status).HasConversion<string>();

        //query filter to auto-ignore soft deleted records
        builder.HasQueryFilter(f => !f.IsDeleted);


    }
}