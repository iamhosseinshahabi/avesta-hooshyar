using CryptoQuote.API.Application.Queries.GetCryptoQuote;
using CryptoQuote.API.Domain.Entities;
using CryptoQuote.API.Domain.Exceptions;
using CryptoQuote.API.Domain.Interfaces;
using CryptoQuote.API.Infrastructure.HttpClients;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CryptoQuote.UnitTests.Application;

[TestClass]
public class GetCryptoQuoteQueryHandlerTests
{
    private readonly Mock<ICoinServiceClient> _coinClientMock = new();
    private readonly Mock<IExchangeServiceClient> _exchangeClientMock = new();
    private readonly Mock<IQuoteHistoryRepository> _repositoryMock = new();

    private GetCryptoQuoteQueryHandler CreateHandler() =>
        new(_coinClientMock.Object,
            _exchangeClientMock.Object,
            _repositoryMock.Object,
            NullLogger<GetCryptoQuoteQueryHandler>.Instance);

    private void SetupSaveSuccess()
    {
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<QuoteHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [TestMethod]
    public async Task Handle_ValidSymbol_ReturnsQuoteWithAllCurrencies()
    {
        // Arrange
        _coinClientMock
            .Setup(x => x.GetPriceInUsdAsync("BTC", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CoinPriceDto("BTC", "Bitcoin", 50_000m));

        _exchangeClientMock
            .Setup(x => x.ConvertFromUsdAsync(50_000m, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConversionDto("USD", 50_000m, new Dictionary<string, decimal>
            {
                ["EUR"] = 46_000m,
                ["BRL"] = 255_000m,
                ["GBP"] = 39_500m,
                ["AUD"] = 76_000m
            }));

        SetupSaveSuccess();
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetCryptoQuoteQuery("BTC"), CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("BTC", result.CryptoCode);
        Assert.AreEqual("Bitcoin", result.CryptoName);
        Assert.AreEqual(50_000m, result.Quotes["USD"]);
        Assert.AreEqual(46_000m, result.Quotes["EUR"]);
        Assert.AreEqual(255_000m, result.Quotes["BRL"]);
        Assert.AreEqual(39_500m, result.Quotes["GBP"]);
        Assert.AreEqual(76_000m, result.Quotes["AUD"]);
    }

    [TestMethod]
    public async Task Handle_UnknownSymbol_ThrowsCryptocurrencyNotFoundException()
    {
        // Arrange
        _coinClientMock
            .Setup(x => x.GetPriceInUsdAsync("XYZ", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CoinPriceDto?)null);

        var handler = CreateHandler();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<CryptocurrencyNotFoundException>(
            () => handler.Handle(new GetCryptoQuoteQuery("XYZ"), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_ExchangeServiceReturnsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        _coinClientMock
            .Setup(x => x.GetPriceInUsdAsync("ETH", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CoinPriceDto("ETH", "Ethereum", 2_500m));

        _exchangeClientMock
            .Setup(x => x.ConvertFromUsdAsync(It.IsAny<decimal>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConversionDto?)null);

        var handler = CreateHandler();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => handler.Handle(new GetCryptoQuoteQuery("ETH"), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_SymbolIsCaseInsensitive_NormalizesToUppercase()
    {
        // Arrange
        _coinClientMock
            .Setup(x => x.GetPriceInUsdAsync("BTC", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CoinPriceDto("BTC", "Bitcoin", 50_000m));

        _exchangeClientMock
            .Setup(x => x.ConvertFromUsdAsync(50_000m, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConversionDto("USD", 50_000m, new Dictionary<string, decimal>
            {
                ["EUR"] = 46_000m, ["BRL"] = 255_000m, ["GBP"] = 39_500m, ["AUD"] = 76_000m
            }));

        SetupSaveSuccess();
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetCryptoQuoteQuery("btc"), CancellationToken.None);

        // Assert
        Assert.AreEqual("BTC", result.CryptoCode);
        _coinClientMock.Verify(x => x.GetPriceInUsdAsync("BTC", It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_PersistsQuoteToRepository()
    {
        // Arrange
        _coinClientMock
            .Setup(x => x.GetPriceInUsdAsync("BTC", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CoinPriceDto("BTC", "Bitcoin", 50_000m));

        _exchangeClientMock
            .Setup(x => x.ConvertFromUsdAsync(50_000m, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConversionDto("USD", 50_000m, new Dictionary<string, decimal>
            {
                ["EUR"] = 46_000m, ["BRL"] = 255_000m, ["GBP"] = 39_500m, ["AUD"] = 76_000m
            }));

        SetupSaveSuccess();
        var handler = CreateHandler();

        // Act
        await handler.Handle(new GetCryptoQuoteQuery("BTC"), CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<QuoteHistory>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
