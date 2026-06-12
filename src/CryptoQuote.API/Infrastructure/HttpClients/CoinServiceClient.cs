using System.Text.Json;

namespace CryptoQuote.API.Infrastructure.HttpClients;

public class CoinServiceClient : ICoinServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CoinServiceClient> _logger;

    public CoinServiceClient(HttpClient httpClient, ILogger<CoinServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CoinPriceDto?> GetPriceInUsdAsync(string symbol, CancellationToken ct = default)
    {
        _logger.LogInformation("Calling CoinService for {Symbol}", symbol);

        var response = await _httpClient.GetAsync($"api/coins/{Uri.EscapeDataString(symbol)}", ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<CoinPriceDto>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
