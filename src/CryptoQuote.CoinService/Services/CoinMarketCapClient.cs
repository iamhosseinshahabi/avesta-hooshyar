using System.Net.Http.Headers;
using System.Text.Json;
using CryptoQuote.CoinService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CryptoQuote.CoinService.Services;

public class CoinMarketCapClient : ICoinMarketCapClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CoinMarketCapClient> _logger;

    public CoinMarketCapClient(HttpClient httpClient, IConfiguration configuration, ILogger<CoinMarketCapClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var apiKey = configuration["CoinMarketCap:ApiKey"]
            ?? throw new InvalidOperationException("CoinMarketCap:ApiKey is not configured.");

        _httpClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<CoinPriceResponse?> GetPriceInUsdAsync(string symbol, CancellationToken ct = default)
    {
        var url = $"v1/cryptocurrency/quotes/latest?symbol={Uri.EscapeDataString(symbol.ToUpperInvariant())}&convert=USD";

        _logger.LogInformation("Requesting CoinMarketCap price for {Symbol}", symbol);

        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(ct);
        var parsed = JsonSerializer.Deserialize<CoinMarketCapApiResponse>(body);

        if (parsed is null || parsed.Status.ErrorCode != 0)
        {
            _logger.LogWarning("CoinMarketCap returned error {Code}: {Message}",
                parsed?.Status.ErrorCode, parsed?.Status.ErrorMessage);
            return null;
        }

        if (!parsed.Data.TryGetValue(symbol.ToUpperInvariant(), out var coinData))
            return null;

        var price = coinData.Quote["USD"].Price;
        return new CoinPriceResponse(coinData.Symbol, coinData.Name, price);
    }
}
