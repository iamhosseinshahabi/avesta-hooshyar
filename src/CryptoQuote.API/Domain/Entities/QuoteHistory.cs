namespace CryptoQuote.API.Domain.Entities;

public class QuoteHistory
{
    public int Id { get; private set; }
    public string CryptoCode { get; private set; } = default!;
    public string CryptoName { get; private set; } = default!;
    public decimal PriceUsd { get; private set; }
    public decimal PriceEur { get; private set; }
    public decimal PriceBrl { get; private set; }
    public decimal PriceGbp { get; private set; }
    public decimal PriceAud { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private QuoteHistory() { }

    public static QuoteHistory Create(
        string cryptoCode,
        string cryptoName,
        decimal priceUsd,
        decimal priceEur,
        decimal priceBrl,
        decimal priceGbp,
        decimal priceAud)
    {
        return new QuoteHistory
        {
            CryptoCode = cryptoCode.ToUpperInvariant(),
            CryptoName = cryptoName,
            PriceUsd = priceUsd,
            PriceEur = priceEur,
            PriceBrl = priceBrl,
            PriceGbp = priceGbp,
            PriceAud = priceAud,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
