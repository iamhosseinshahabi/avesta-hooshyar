using CryptoQuote.API.Domain.Entities;
using CryptoQuote.API.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CryptoQuote.API.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<QuoteHistory> QuoteHistories => Set<QuoteHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new QuoteHistoryConfiguration());
    }
}
