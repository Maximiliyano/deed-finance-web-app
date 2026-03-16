using Deed.Domain.Entities;
using Deed.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class IncomeTagConfiguration : IEntityTypeConfiguration<IncomeTag>
{
    public void Configure(EntityTypeBuilder<IncomeTag> builder)
    {
        builder.ToTable(TableConfigurationConstants.IncomeTags);

        builder.HasKey(it => new { it.Id, it.TagId });

        builder.HasOne(it => it.Income)
               .WithMany(i => i.Tags)
               .HasForeignKey(it => it.Id);

        builder.HasOne(it => it.Tag)
               .WithMany(t => t.IncomeTags)
               .HasForeignKey(it => it.TagId);

        builder.Property(it => it.IsDeleted)
            .HasDefaultValue(false);

        builder.HasQueryFilter(it => !it.IsDeleted && !it.Income.IsDeleted);
    }
}
