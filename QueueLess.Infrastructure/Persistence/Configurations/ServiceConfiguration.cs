using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QueueLess.Domain.Entities;

namespace QueueLess.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        //One-to-Many: Facility has many Service.
        //Prevent deleting a facility if it has active service configured
        builder.HasOne(s => s.Facility)
            .WithMany(f => f.Services)
            .HasForeignKey(s => s.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}