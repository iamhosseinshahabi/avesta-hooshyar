using CryptoQuote.API.Domain.Entities;

namespace CryptoQuote.API.Domain.Interfaces;

public interface IQuoteHistoryRepository
{
    Task AddAsync(QuoteHistory entity, CancellationToken ct = default);
    Task<IReadOnlyList<QuoteHistory>> GetByCodeAsync(string cryptoCode, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
