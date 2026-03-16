using Deed.Domain.Entities;
using Deed.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deed.Infrastructure.Persistence.Configurations;

internal sealed class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable(TableConfigurationConstants.UserSettings);

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Salary)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.Currency)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.BalanceReminderEnabled)
            .HasDefaultValue(false);

        builder.Property(s => s.BalanceReminderCron)
            .HasMaxLength(128);

        builder.Property(s => s.ExpenseReminderEnabled)
            .HasDefaultValue(false);

        builder.Property(s => s.ExpenseReminderCron)
            .HasMaxLength(128);

        builder.Property(s => s.DebtReminderEnabled)
            .HasDefaultValue(false);

        builder.Property(s => s.DebtReminderCron)
            .HasMaxLength(128);

        builder.Property(s => s.EmailNotificationsEnabled)
            .HasDefaultValue(false);

        builder.Property(s => s.Email)
            .HasMaxLength(256);

        builder.Property(s => s.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.UpdatedBy)
            .HasMaxLength(256);

        builder.HasIndex(s => s.CreatedBy)
            .IsUnique();
    }
}
