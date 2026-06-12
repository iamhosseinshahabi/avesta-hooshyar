namespace CryptoQuote.ExchangeService.Models;

public record ConversionResponse(
    string FromCurrency,
    decimal OriginalAmount,
    Dictionary<string, decimal> ConvertedAmounts);
