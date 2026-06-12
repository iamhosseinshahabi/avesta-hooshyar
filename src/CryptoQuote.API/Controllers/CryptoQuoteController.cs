using CryptoQuote.API.Application.Queries.GetCryptoQuote;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CryptoQuote.API.Controllers;

/// <summary>
/// Provides real-time cryptocurrency price quotes in multiple fiat currencies.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CryptoQuoteController : ControllerBase
{
    private readonly ISender _sender;

    public CryptoQuoteController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves the latest quotes for a cryptocurrency in USD, EUR, BRL, GBP, and AUD.
    /// </summary>
    /// <remarks>
    /// Fetches the current USD price from CoinMarketCap, then converts it to the remaining
    /// currencies via ExchangeRates API. Each successful call is persisted to the database.
    ///
    /// Example request:
    ///
    ///     GET /api/cryptoquote/BTC
    ///
    /// Example response:
    ///
    ///     {
    ///       "cryptoCode": "BTC",
    ///       "cryptoName": "Bitcoin",
    ///       "quotes": {
    ///         "USD": 68000.00,
    ///         "EUR": 62800.00,
    ///         "BRL": 340000.00,
    ///         "GBP": 53600.00,
    ///         "AUD": 103700.00
    ///       },
    ///       "retrievedAtUtc": "2024-06-11T10:00:00Z"
    ///     }
    /// </remarks>
    /// <param name="cryptoCode">
    /// Ticker symbol of the cryptocurrency (case-insensitive). Examples: <c>BTC</c>, <c>ETH</c>, <c>USDT</c>.
    /// </param>
    /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
    /// <returns>Current price quotes in all supported currencies.</returns>
    /// <response code="200">Quotes retrieved successfully.</response>
    /// <response code="400">
    /// The <paramref name="cryptoCode"/> failed validation (e.g. empty, too long, or contains invalid characters).
    /// </response>
    /// <response code="404">No cryptocurrency with the given symbol was found on CoinMarketCap.</response>
    /// <response code="502">A downstream service (CoinMarketCap or ExchangeRates) is temporarily unavailable.</response>
    [HttpGet("{cryptoCode}")]
    [ProducesResponseType(typeof(GetCryptoQuoteResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetQuote(string cryptoCode, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCryptoQuoteQuery(cryptoCode), cancellationToken);
        return Ok(result);
    }
}
