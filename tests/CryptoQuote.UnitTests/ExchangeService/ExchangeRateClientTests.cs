using System.Net;
using System.Text.Json;
using CryptoQuote.ExchangeService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace CryptoQuote.UnitTests.ExchangeService;

[TestClass]
public class ExchangeRateClientTests
{
    private static IConfiguration BuildConfig(string accessKey = "test-key") =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ExchangeRates:AccessKey"] = accessKey
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

        return new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://api.exchangeratesapi.io/") };
    }

    [TestMethod]
    public async Task ConvertFromUsdAsync_ValidResponse_ReturnsConvertedAmounts()
    {
        // Arrange – EUR base: 1 EUR = 1.10 USD, 1 EUR = 0.86 GBP
        // EUR rate = 1.0 because base is EUR (1 EUR = 1 EUR)
        var json = JsonSerializer.Serialize(new
        {
            success = true,
            @base = "EUR",
            date = "2024-01-01",
            rates = new { USD = 1.10m, EUR = 1.0m, GBP = 0.86m, BRL = 5.5m, AUD = 1.65m }
        });

        var client = new ExchangeRateClient(
            BuildHttpClient(HttpStatusCode.OK, json),
            BuildConfig(),
            NullLogger<ExchangeRateClient>.Instance);

        // Act
        var result = await client.ConvertFromUsdAsync(1100m, ["EUR", "GBP", "BRL", "AUD"]);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("USD", result.FromCurrency);
        Assert.AreEqual(1100m, result.OriginalAmount);

        // 1100 USD / 1.10 USD/EUR = 1000 EUR
        Assert.AreEqual(1000m, result.ConvertedAmounts["EUR"], 0.01m);

        // 1100 * 0.86 / 1.10 ≈ 860 GBP
        Assert.AreEqual(860m, result.ConvertedAmounts["GBP"], 0.01m);
    }

    [TestMethod]
    public async Task ConvertFromUsdAsync_ApiFailure_ReturnsNull()
    {
        // Arrange
        var json = JsonSerializer.Serialize(new
        {
            success = false,
            error = new { code = 101, info = "No API Key was specified." }
        });

        var client = new ExchangeRateClient(
            BuildHttpClient(HttpStatusCode.OK, json),
            BuildConfig(),
            NullLogger<ExchangeRateClient>.Instance);

        // Act
        var result = await client.ConvertFromUsdAsync(1000m, ["EUR", "GBP"]);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ConvertFromUsdAsync_ResultAlwaysContainsUsd()
    {
        // Arrange
        var json = JsonSerializer.Serialize(new
        {
            success = true,
            @base = "EUR",
            date = "2024-01-01",
            rates = new { USD = 1.10m, EUR = 1.0m }
        });

        var client = new ExchangeRateClient(
            BuildHttpClient(HttpStatusCode.OK, json),
            BuildConfig(),
            NullLogger<ExchangeRateClient>.Instance);

        // Act
        var result = await client.ConvertFromUsdAsync(500m, ["EUR"]);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.ConvertedAmounts.ContainsKey("USD"));
        Assert.AreEqual(500m, result.ConvertedAmounts["USD"]);
    }
}
