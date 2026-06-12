using CryptoQuote.API.Application.Queries.GetCryptoQuote;
using CryptoQuote.API.Application.Validators;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CryptoQuote.UnitTests.Application;

[TestClass]
public class GetCryptoQuoteQueryValidatorTests
{
    private readonly GetCryptoQuoteQueryValidator _validator = new();

    [DataTestMethod]
    [DataRow("BTC")]
    [DataRow("ETH")]
    [DataRow("USDT")]
    [DataRow("A")]
    public void Validate_ValidCryptoCode_NoErrors(string code)
    {
        var result = _validator.TestValidate(new GetCryptoQuoteQuery(code));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void Validate_EmptyCode_HasError(string code)
    {
        var result = _validator.TestValidate(new GetCryptoQuoteQuery(code));
        result.ShouldHaveValidationErrorFor(x => x.CryptoCode);
    }

    [TestMethod]
    public void Validate_TooLongCode_HasError()
    {
        var result = _validator.TestValidate(new GetCryptoQuoteQuery("TOOLONGCODE1"));
        result.ShouldHaveValidationErrorFor(x => x.CryptoCode);
    }

    [DataTestMethod]
    [DataRow("BTC!")]
    [DataRow("BTC USD")]
    [DataRow("BTC-USD")]
    public void Validate_InvalidCharacters_HasError(string code)
    {
        var result = _validator.TestValidate(new GetCryptoQuoteQuery(code));
        result.ShouldHaveValidationErrorFor(x => x.CryptoCode);
    }
}
