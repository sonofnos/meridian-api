using Meridian.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meridian.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Amount).HasPrecision(18, 4);
        builder.Property(t => t.Currency).IsRequired().HasMaxLength(3);
        builder.Property(t => t.Reference).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.Property(t => t.Status).HasConversion<string>();
        builder.Property(t => t.FailureReason).HasMaxLength(1000);
        builder.HasIndex(t => t.Reference).IsUnique();
        builder.HasIndex(t => t.DebitAccountId);
        builder.HasIndex(t => t.CreditAccountId);
        builder.ToTable("transactions");
    }
}
