namespace CryptoQuote.API.Infrastructure.HttpClients;

public interface IExchangeServiceClient
{
    Task<ConversionDto?> ConvertFromUsdAsync(
        decimal amount,
        IEnumerable<string> targetCurrencies,
        CancellationToken ct = default);
}

public record ConversionDto(
    string FromCurrency,
    decimal OriginalAmount,
    Dictionary<string, decimal> ConvertedAmounts);
