using CryptoQuote.CoinService.Models;

namespace CryptoQuote.CoinService.Services;

public interface ICoinMarketCapClient
{
    Task<CoinPriceResponse?> GetPriceInUsdAsync(string symbol, CancellationToken ct = default);
}
