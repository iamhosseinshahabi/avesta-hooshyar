using MediatR;

namespace CryptoQuote.API.Application.Queries.GetCryptoQuote;

/// <summary>
/// CQRS query that retrieves the latest price of a cryptocurrency in all supported fiat currencies.
/// </summary>
/// <param name="CryptoCode">Ticker symbol to look up, e.g. <c>BTC</c>.</param>
public record GetCryptoQuoteQuery(string CryptoCode) : IRequest<GetCryptoQuoteResult>;
