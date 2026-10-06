using FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowOps.Modules.Procurement.Domain.Tests.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Create_WithNegativeAmount_ShouldFail()
    {
        var currencyResult =
            Currency.Create("EUR");

        Assert.True(currencyResult.IsSuccess);

        var result =
            Money.Create(
                -1m,
                currencyResult.Value);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "Money.NegativeAmount",
            result.Error.Code);
    }

    [Fact]
    public void Multiply_ShouldReturnCorrectAmount()
    {
        var currencyResult =
            Currency.Create("EUR");

        Assert.True(currencyResult.IsSuccess);

        var moneyResult =
            Money.Create(
                100m,
                currencyResult.Value);

        Assert.True(moneyResult.IsSuccess);

        var total =
            moneyResult.Value.Multiply(3);

        Assert.Equal(
            300m,
            total.Amount);

        Assert.Equal(
            currencyResult.Value,
            total.Currency);
    }
}
