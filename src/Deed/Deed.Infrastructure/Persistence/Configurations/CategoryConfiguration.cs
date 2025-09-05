using System.Text;
using System.Text.Json;
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
        builder.HasKey(c => c.Id);

        builder.HasData(Seeder.Parse<Category>(SeederConstants.Categories));

        builder.ToTable(TableConfigurationConstants.Categories);
    }
}
