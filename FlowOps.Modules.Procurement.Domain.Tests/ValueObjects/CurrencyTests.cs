using FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;

namespace FlowOps.Modules.Procurement.Domain.Tests.ValueObjects;

public sealed class CurrencyTests
{
    [Theory]
    [InlineData("EUR")]
    [InlineData("USD")]
    [InlineData("IRR")]
    [InlineData("eur")]
    [InlineData(" usd ")]
    public void Create_WithSupportedCurrency_ShouldSucceed(
        string code)
    {
        var result =
            Currency.Create(code);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithUnsupportedCurrency_ShouldFail()
    {
        var result =
            Currency.Create("ABC");

        Assert.True(result.IsFailure);

        Assert.Equal(
            "Currency.Unsupported",
            result.Error.Code);
    }
}