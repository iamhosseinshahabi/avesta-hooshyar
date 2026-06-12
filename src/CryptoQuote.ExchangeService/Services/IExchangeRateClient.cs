using CryptoQuote.ExchangeService.Models;

namespace CryptoQuote.ExchangeService.Services;

public interface IExchangeRateClient
{
    /// <summary>
    /// Converts <paramref name="amount"/> in USD to each of the <paramref name="targetCurrencies"/>.
    /// </summary>
    Task<ConversionResponse?> ConvertFromUsdAsync(
        decimal amount,
        IEnumerable<string> targetCurrencies,
        CancellationToken ct = default);
}
