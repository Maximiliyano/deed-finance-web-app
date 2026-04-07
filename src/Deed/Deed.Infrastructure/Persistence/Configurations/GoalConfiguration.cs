using Deed.Domain.Entities;
using Deed.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable(TableConfigurationConstants.Goals);

        builder.Property(g => g.IsDeleted)
            .HasDefaultValue(false);

        builder.HasQueryFilter(g => !g.IsDeleted);

        builder.HasIndex(g => g.IsDeleted)
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(g => new { g.CreatedBy, g.IsDeleted });

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Title)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(g => g.TargetAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(g => g.CurrentAmount)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.Property(g => g.Currency)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(g => g.Note)
            .HasMaxLength(512);

        builder.Property(g => g.IsCompleted)
            .HasDefaultValue(false);

        builder.Property(g => g.OrderIndex)
            .HasDefaultValue(0);

        builder.Property(g => g.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(g => g.UpdatedBy)
            .HasMaxLength(256);
    }
}
