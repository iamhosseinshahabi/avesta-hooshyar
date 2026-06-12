using System.Text.Json.Serialization;

namespace CryptoQuote.CoinService.Models;

public class CoinMarketCapApiResponse
{
    [JsonPropertyName("status")]
    public CmcStatus Status { get; set; } = default!;

    [JsonPropertyName("data")]
    public Dictionary<string, CmcCoinData> Data { get; set; } = [];
}

public class CmcStatus
{
    [JsonPropertyName("error_code")]
    public int ErrorCode { get; set; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }
}

public class CmcCoinData
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = default!;

    [JsonPropertyName("quote")]
    public Dictionary<string, CmcQuote> Quote { get; set; } = [];
}

public class CmcQuote
{
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}
