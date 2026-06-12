namespace CryptoQuote.CoinService.Models;

public record CoinPriceResponse(string Symbol, string Name, decimal PriceUsd);
