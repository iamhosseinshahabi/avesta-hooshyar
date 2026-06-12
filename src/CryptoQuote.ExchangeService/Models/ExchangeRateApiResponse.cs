using System.Text.Json.Serialization;

namespace CryptoQuote.ExchangeService.Models;

public class ExchangeRateApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("base")]
    public string Base { get; set; } = default!;

    [JsonPropertyName("date")]
    public string Date { get; set; } = default!;

    [JsonPropertyName("rates")]
    public Dictionary<string, decimal> Rates { get; set; } = [];

    [JsonPropertyName("error")]
    public ExchangeRateError? Error { get; set; }
}

public class ExchangeRateError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("info")]
    public string Info { get; set; } = default!;
}
