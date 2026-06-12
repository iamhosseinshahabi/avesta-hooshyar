using CryptoQuote.API.Domain.Entities;
using CryptoQuote.API.Domain.Exceptions;
using CryptoQuote.API.Domain.Interfaces;
using CryptoQuote.API.Infrastructure.HttpClients;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoQuote.API.Application.Queries.GetCryptoQuote;

public class GetCryptoQuoteQueryHandler : IRequestHandler<GetCryptoQuoteQuery, GetCryptoQuoteResult>
{
    private static readonly string[] TargetCurrencies = ["EUR", "BRL", "GBP", "AUD"];

    private readonly ICoinServiceClient _coinServiceClient;
    private readonly IExchangeServiceClient _exchangeServiceClient;
    private readonly IQuoteHistoryRepository _repository;
    private readonly ILogger<GetCryptoQuoteQueryHandler> _logger;

    public GetCryptoQuoteQueryHandler(
        ICoinServiceClient coinServiceClient,
        IExchangeServiceClient exchangeServiceClient,
        IQuoteHistoryRepository repository,
        ILogger<GetCryptoQuoteQueryHandler> logger)
    {
        _coinServiceClient = coinServiceClient;
        _exchangeServiceClient = exchangeServiceClient;
        _repository = repository;
        _logger = logger;
    }

    public async Task<GetCryptoQuoteResult> Handle(GetCryptoQuoteQuery request, CancellationToken cancellationToken)
    {
        var symbol = request.CryptoCode.ToUpperInvariant();
        _logger.LogInformation("Handling GetCryptoQuoteQuery for {Symbol}", symbol);

        // Step 1: fetch USD price from CoinService
        var coinPrice = await _coinServiceClient.GetPriceInUsdAsync(symbol, cancellationToken);
        if (coinPrice is null)
            throw new CryptocurrencyNotFoundException(symbol);

        // Step 2: convert to target currencies via ExchangeService
        var conversion = await _exchangeServiceClient.ConvertFromUsdAsync(
            coinPrice.PriceUsd, TargetCurrencies, cancellationToken);

        if (conversion is null)
            throw new InvalidOperationException("Exchange rate service returned an empty response.");

        // Merge USD + converted currencies into one map
        var quotes = new Dictionary<string, decimal>(conversion.ConvertedAmounts)
        {
            ["USD"] = coinPrice.PriceUsd
        };

        // Step 3: persist to SQL Server
        var history = QuoteHistory.Create(
            cryptoCode: coinPrice.Symbol,
            cryptoName: coinPrice.Name,
            priceUsd: quotes.GetValueOrDefault("USD"),
            priceEur: quotes.GetValueOrDefault("EUR"),
            priceBrl: quotes.GetValueOrDefault("BRL"),
            priceGbp: quotes.GetValueOrDefault("GBP"),
            priceAud: quotes.GetValueOrDefault("AUD"));

        await _repository.AddAsync(history, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Quote for {Symbol} persisted (id={Id})", symbol, history.Id);

        return new GetCryptoQuoteResult(
            CryptoCode: coinPrice.Symbol,
            CryptoName: coinPrice.Name,
            Quotes: quotes,
            RetrievedAtUtc: DateTime.UtcNow);
    }
}
