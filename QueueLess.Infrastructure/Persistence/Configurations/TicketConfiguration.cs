using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QueueLess.Domain.Entities;

namespace QueueLess.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TicketNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.CustomerId)
            .IsRequired();

        builder.Property(t => t.State)
            .HasConversion<string>();

        // Ensure the sequence number index remains unique per service per day
        builder.HasIndex(t => new { t.ServiceId, t.SequenceNumber, t.CreatedAt }).IsUnique();

        // Service has many Tickets. Prevent deleting a Service with active Tickets.
        builder.HasOne(t => t.Service)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}