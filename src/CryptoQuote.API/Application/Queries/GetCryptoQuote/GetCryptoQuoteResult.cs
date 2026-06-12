namespace CryptoQuote.API.Application.Queries.GetCryptoQuote;

/// <summary>
/// The response returned by <c>GET /api/cryptoquote/{cryptoCode}</c>.
/// </summary>
/// <param name="CryptoCode">Ticker symbol as recognised by CoinMarketCap, e.g. <c>BTC</c>.</param>
/// <param name="CryptoName">Full display name of the cryptocurrency, e.g. <c>Bitcoin</c>.</param>
/// <param name="Quotes">
/// Dictionary of currency code → price, always containing USD, EUR, BRL, GBP, and AUD.
/// Amounts are rounded to 8 decimal places.
/// </param>
/// <param name="RetrievedAtUtc">UTC timestamp of when the quote was fetched.</param>
public record GetCryptoQuoteResult(
    string CryptoCode,
    string CryptoName,
    Dictionary<string, decimal> Quotes,
    DateTime RetrievedAtUtc);
