using System.Text.Json;

namespace CryptoQuote.API.Infrastructure.HttpClients;

public class ExchangeServiceClient : IExchangeServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExchangeServiceClient> _logger;

    public ExchangeServiceClient(HttpClient httpClient, ILogger<ExchangeServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ConversionDto?> ConvertFromUsdAsync(
        decimal amount,
        IEnumerable<string> targetCurrencies,
        CancellationToken ct = default)
    {
        var targets = string.Join(",", targetCurrencies);
        var url = $"api/exchange/convert?amount={amount}&targets={Uri.EscapeDataString(targets)}";

        _logger.LogInformation("Calling ExchangeService for amount={Amount}, targets={Targets}", amount, targets);

        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<ConversionDto>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
