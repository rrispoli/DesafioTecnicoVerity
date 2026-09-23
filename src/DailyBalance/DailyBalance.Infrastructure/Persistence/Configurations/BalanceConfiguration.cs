using DailyBalance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DailyBalance.Infrastructure.Persistence.Configurations;

internal sealed class BalanceConfiguration : IEntityTypeConfiguration<Balance>
{
    public void Configure(EntityTypeBuilder<Balance> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Date).IsUnique();

        builder.Property(x => x.Credit)
            .HasPrecision(18, 2);

        builder.Property(x => x.Debit)
            .HasPrecision(18, 2);
    }
}