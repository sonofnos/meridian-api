using Meridian.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meridian.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.OwnerId).IsRequired().HasMaxLength(100);
        builder.Property(a => a.OwnerName).IsRequired().HasMaxLength(200);
        builder.Property(a => a.AccountNumber).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Currency).IsRequired().HasMaxLength(3);
        builder.Property(a => a.Balance).HasPrecision(18, 4);
        builder.Property(a => a.Status).HasConversion<string>();
        builder.HasIndex(a => a.AccountNumber).IsUnique();
        builder.HasIndex(a => a.OwnerId);
        builder.ToTable("accounts");
    }
}
