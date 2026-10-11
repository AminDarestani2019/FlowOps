using FlowOps.Modules.Procurement.Domain.PurchaseRequests;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;

namespace FlowOps.Modules.Procurement.Domain.Tests.PurchaseRequests;

internal static class PurchaseRequestTestData
{
    public static PurchaseRequest CreateDraft()
    {
        var numberResult =
            PurchaseRequestNumber.Create("PR-2026-000001");

        Assert.True(numberResult.IsSuccess);

        var currencyResult =
            Currency.Create("EUR");

        Assert.True(currencyResult.IsSuccess);

        var result = PurchaseRequest.Create(
            numberResult.Value,
            EmployeeId.New(),
            DepartmentId.New(),
            currencyResult.Value,
            new DateTime(
                2026,
                10,
                5,
                8,
                0,
                0,
                DateTimeKind.Utc));

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    public static Currency CreateEur()
    {
        var result = Currency.Create("EUR");

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    public static Money CreateMoney(
        decimal amount,
        string currency = "EUR")
    {
        var currencyResult =
            Currency.Create(currency);

        Assert.True(currencyResult.IsSuccess);

        var moneyResult =
            Money.Create(
                amount,
                currencyResult.Value);

        Assert.True(moneyResult.IsSuccess);

        return moneyResult.Value;
    }

    public static PurchaseRequest CreateSubmitted()
    {
        var request = CreateDraft();

        var money =
            CreateMoney(1500m);

        var addResult =
            request.AddItem(
                "Development Laptop",
                1,
                money);

        Assert.True(addResult.IsSuccess);

        var submitResult =
            request.Submit(
                new DateTime(
                    2026,
                    10,
                    10,
                    8,
                    0,
                    0,
                    DateTimeKind.Utc));

        Assert.True(submitResult.IsSuccess);

        return request;
    }

    public static PurchaseRequest CreateInApproval()
    {
        var request =
            CreateSubmitted();

        var result =
            request.MarkInApproval();

        Assert.True(result.IsSuccess);

        return request;
    }
}
