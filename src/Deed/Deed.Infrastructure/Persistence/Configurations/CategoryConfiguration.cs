using System.Text;
using System.Text.Json;
using Deed.Domain.Constants;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Infrastructure.Persistence.Constants;
using Deed.Infrastructure.Persistence.DataSeed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Options;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable(TableConfigurationConstants.Categories);

        builder.HasQueryFilter(c =>
            !c.IsDeleted.HasValue ||
            c.IsDeleted.HasValue && !c.IsDeleted.Value);

        builder.HasIndex(t => t.IsDeleted)
            .HasFilter("is_deleted = 0");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxLenghtName);

        builder.Property(c => c.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.PlannedPeriodAmount)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(c => c.Period)
            .HasConversion<int>();

        builder.HasMany(x => x.Incomes)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Expenses)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(Seeder.Parse<Category>(SeederConstants.Categories));
    }
}
