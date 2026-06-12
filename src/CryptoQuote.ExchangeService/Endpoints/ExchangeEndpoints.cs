using CryptoQuote.ExchangeService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptoQuote.ExchangeService.Endpoints;

public static class ExchangeEndpoints
{
    private static readonly string[] DefaultTargets = ["EUR", "BRL", "GBP", "AUD"];

    public static void MapExchangeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/exchange").WithTags("Exchange");

        group.MapGet("/convert", async (
            [FromQuery] decimal amount,
            IExchangeRateClient client,
            CancellationToken ct,
            [FromQuery] string? targets = null) =>
        {
            if (amount <= 0)
                return Results.BadRequest("Amount must be greater than zero.");

            var targetList = targets?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                ?? DefaultTargets;

            var result = await client.ConvertFromUsdAsync(amount, targetList, ct);

            return result is null
                ? Results.StatusCode(StatusCodes.Status502BadGateway)
                : Results.Ok(result);
        })
        .WithName("ConvertFromUsd")
        .WithSummary("Convert a USD amount to EUR, BRL, GBP, AUD");
    }
}
