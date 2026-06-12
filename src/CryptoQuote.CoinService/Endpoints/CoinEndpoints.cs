using CryptoQuote.CoinService.Services;

namespace CryptoQuote.CoinService.Endpoints;

public static class CoinEndpoints
{
    public static void MapCoinEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/coins").WithTags("Coins");

        group.MapGet("/{symbol}", async (
            string symbol,
            ICoinMarketCapClient client,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(symbol) || symbol.Length > 10)
                return Results.BadRequest("Symbol must be between 1 and 10 characters.");

            var result = await client.GetPriceInUsdAsync(symbol.ToUpperInvariant(), ct);

            return result is null
                ? Results.NotFound($"Cryptocurrency '{symbol}' not found.")
                : Results.Ok(result);
        })
        .WithName("GetCoinPrice")
        .WithSummary("Get cryptocurrency price in USD");
    }
}
