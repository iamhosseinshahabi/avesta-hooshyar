using CryptoQuote.API.Application.Queries.GetCryptoQuote;
using FluentValidation;

namespace CryptoQuote.API.Application.Validators;

public class GetCryptoQuoteQueryValidator : AbstractValidator<GetCryptoQuoteQuery>
{
    public GetCryptoQuoteQueryValidator()
    {
        RuleFor(x => x.CryptoCode)
            .NotEmpty().WithMessage("Crypto code is required.")
            .MaximumLength(10).WithMessage("Crypto code must not exceed 10 characters.")
            .Matches("^[A-Za-z0-9]+$").WithMessage("Crypto code must contain only alphanumeric characters.");
    }
}
