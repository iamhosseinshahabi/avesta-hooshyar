using System.Net;
using System.Text.Json;
using CryptoQuote.CoinService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace CryptoQuote.UnitTests.CoinService;

[TestClass]
public class CoinMarketCapClientTests
{
    private static IConfiguration BuildConfig(string apiKey = "test-key") =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CoinMarketCap:ApiKey"] = apiKey
            })
            .Build();

    private static HttpClient BuildHttpClient(HttpStatusCode status, string content)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = status,
                Content = new StringContent(content)
            });

        return new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://pro-api.coinmarketcap.com/") };
    }

    [TestMethod]
    public async Task GetPriceInUsdAsync_ValidResponse_ReturnsCoinPrice()
    {
        // Arrange
        var json = JsonSerializer.Serialize(new
        {
            status = new { error_code = 0, error_message = (string?)null },
            data = new
            {
                BTC = new
                {
                    name = "Bitcoin",
                    symbol = "BTC",
                    quote = new { USD = new { price = 50000.0m } }
                }
            }
        });

        var client = new CoinMarketCapClient(
            BuildHttpClient(HttpStatusCode.OK, json),
            BuildConfig(),
            NullLogger<CoinMarketCapClient>.Instance);

        // Act
        var result = await client.GetPriceInUsdAsync("BTC");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("BTC", result.Symbol);
        Assert.AreEqual("Bitcoin", result.Name);
        Assert.AreEqual(50000.0m, result.PriceUsd);
    }

    [TestMethod]
    public async Task GetPriceInUsdAsync_UnknownSymbol_ReturnsNull()
    {
        // Arrange
        var json = JsonSerializer.Serialize(new
        {
            status = new { error_code = 0, error_message = (string?)null },
            data = new Dictionary<string, object>()
        });

        var client = new CoinMarketCapClient(
            BuildHttpClient(HttpStatusCode.OK, json),
            BuildConfig(),
            NullLogger<CoinMarketCapClient>.Instance);

        // Act
        var result = await client.GetPriceInUsdAsync("UNKNOWN");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetPriceInUsdAsync_ApiError_ReturnsNull()
    {
        // Arrange
        var json = JsonSerializer.Serialize(new
        {
            status = new { error_code = 400, error_message = "Invalid value for 'symbol'" },
            data = new Dictionary<string, object>()
        });

        var client = new CoinMarketCapClient(
            BuildHttpClient(HttpStatusCode.OK, json),
            BuildConfig(),
            NullLogger<CoinMarketCapClient>.Instance);

        // Act
        var result = await client.GetPriceInUsdAsync("INVALID!");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Constructor_MissingApiKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() =>
            new CoinMarketCapClient(
                new HttpClient(),
                config,
                NullLogger<CoinMarketCapClient>.Instance));
    }
}
