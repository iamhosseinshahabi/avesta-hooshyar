namespace CryptoQuote.API.Domain.Exceptions;

public class CryptocurrencyNotFoundException : Exception
{
    public CryptocurrencyNotFoundException(string symbol)
        : base($"Cryptocurrency '{symbol}' was not found.") { }
}
