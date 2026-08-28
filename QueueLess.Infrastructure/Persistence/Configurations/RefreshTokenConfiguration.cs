using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QueueLess.Domain.Entities;

namespace QueueLess.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
            .IsRequired();

        builder.Property(rt => rt.JwtId)
            .IsRequired();

        builder.Property(rt => rt.UserId)
            .IsRequired();

        // Enforce soft delete filters
        builder.HasQueryFilter(rt => !rt.IsDeleted);
    }
}