using CryptoQuote.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoQuote.API.Infrastructure.Data.Configurations;

public class QuoteHistoryConfiguration : IEntityTypeConfiguration<QuoteHistory>
{
    public void Configure(EntityTypeBuilder<QuoteHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CryptoCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.CryptoName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PriceUsd).HasColumnType("decimal(28,8)");
        builder.Property(x => x.PriceEur).HasColumnType("decimal(28,8)");
        builder.Property(x => x.PriceBrl).HasColumnType("decimal(28,8)");
        builder.Property(x => x.PriceGbp).HasColumnType("decimal(28,8)");
        builder.Property(x => x.PriceAud).HasColumnType("decimal(28,8)");

        builder.HasIndex(x => x.CryptoCode);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
