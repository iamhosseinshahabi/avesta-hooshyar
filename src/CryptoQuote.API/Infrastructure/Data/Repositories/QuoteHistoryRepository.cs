using CryptoQuote.API.Domain.Entities;
using CryptoQuote.API.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CryptoQuote.API.Infrastructure.Data.Repositories;

public class QuoteHistoryRepository : IQuoteHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public QuoteHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(QuoteHistory entity, CancellationToken ct = default)
        => await _context.QuoteHistories.AddAsync(entity, ct);

    public async Task<IReadOnlyList<QuoteHistory>> GetByCodeAsync(string cryptoCode, CancellationToken ct = default)
        => await _context.QuoteHistories
            .Where(x => x.CryptoCode == cryptoCode.ToUpperInvariant())
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
