namespace CryptoQuote.API.Infrastructure.HttpClients;

public interface ICoinServiceClient
{
    Task<CoinPriceDto?> GetPriceInUsdAsync(string symbol, CancellationToken ct = default);
}

public record CoinPriceDto(string Symbol, string Name, decimal PriceUsd);
