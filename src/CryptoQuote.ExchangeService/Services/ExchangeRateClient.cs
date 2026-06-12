using System.Text.Json;
using CryptoQuote.ExchangeService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CryptoQuote.ExchangeService.Services;

/// <summary>
/// Calls exchangeratesapi.io (free tier uses EUR as base; conversion to USD base is done here).
/// </summary>
public class ExchangeRateClient : IExchangeRateClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExchangeRateClient> _logger;

    public ExchangeRateClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ExchangeRateClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ConversionResponse?> ConvertFromUsdAsync(
        decimal amount,
        IEnumerable<string> targetCurrencies,
        CancellationToken ct = default)
    {
        var accessKey = _configuration["ExchangeRates:AccessKey"]
            ?? throw new InvalidOperationException("ExchangeRates:AccessKey is not configured.");

        // Free tier only supports EUR base; we always fetch USD as well to rebase.
        var symbols = string.Join(",", targetCurrencies.Append("USD").Distinct());
        var url = $"v1/latest?access_key={accessKey}&symbols={symbols}";

        _logger.LogInformation("Requesting exchange rates for symbols: {Symbols}", symbols);

        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(ct);
        var parsed = JsonSerializer.Deserialize<ExchangeRateApiResponse>(body);

        if (parsed is null || !parsed.Success)
        {
            _logger.LogWarning("ExchangeRates API returned failure: {Info}", parsed?.Error?.Info);
            return null;
        }

        // Rates are relative to EUR. We rebase to USD:
        //   rate_eur_usd  = rates["USD"]  (1 EUR = N USD)
        //   rate_X_usd    = rates["X"] / rates["USD"]   (1 USD = ? X)
        if (!parsed.Rates.TryGetValue("USD", out var eurToUsd) || eurToUsd == 0)
            return null;

        var converted = new Dictionary<string, decimal>();
        foreach (var (currency, eurRate) in parsed.Rates)
        {
            if (currency == "USD") continue;
            // amount_usd * (eurRate / eurToUsd) = amount in target currency
            converted[currency] = Math.Round(amount * (eurRate / eurToUsd), 2);
        }
        // USD itself
        converted["USD"] = Math.Round(amount, 2);

        return new ConversionResponse("USD", amount, converted);
    }
}
