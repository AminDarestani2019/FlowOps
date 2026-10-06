namespace FlowOps.Modules.Procurement.Domain.Tests.PurchaseRequests;

public sealed class PurchaseRequestItemTests
{
    [Fact]
    public void AddItem_WithDifferentCurrency_ShouldFail()
    {
        var request =
            PurchaseRequestTestData.CreateDraft();

        var usdMoney =
            PurchaseRequestTestData.CreateMoney(
                1000m,
                "USD");

        var result = request.AddItem(
            "Laptop",
            1,
            usdMoney);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "PurchaseRequest.CurrencyMismatch",
            result.Error.Code);

        Assert.Empty(
            request.Items);
    }

    [Fact]
    public void AddItem_WithZeroQuantity_ShouldFail()
    {
        var request =
            PurchaseRequestTestData.CreateDraft();

        var money =
            PurchaseRequestTestData.CreateMoney(100m);

        var result = request.AddItem(
            "Keyboard",
            0,
            money);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "PurchaseRequest.InvalidQuantity",
            result.Error.Code);

        Assert.Empty(
            request.Items);
    }

    [Fact]
    public void AddItem_WithValidData_ShouldAddItem()
    {
        var request =
            PurchaseRequestTestData.CreateDraft();

        var money =
            PurchaseRequestTestData.CreateMoney(1500m);

        var result = request.AddItem(
            "Dell Laptop",
            2,
            money);

        Assert.True(result.IsSuccess);

        var item = Assert.Single(request.Items);

        Assert.Equal(
            "Dell Laptop",
            item.Description);

        Assert.Equal(
            2,
            item.Quantity);

        Assert.Equal(
            1500m,
            item.UnitPrice.Amount);
    }

    [Fact]
    public void AddItem_ShouldUpdateTotalAmount()
    {
        var request =
            PurchaseRequestTestData.CreateDraft();

        var laptopPrice =
            PurchaseRequestTestData.CreateMoney(1500m);

        var monitorPrice =
            PurchaseRequestTestData.CreateMoney(500m);

        request.AddItem(
            "Laptop",
            2,
            laptopPrice);

        request.AddItem(
            "Monitor",
            3,
            monitorPrice);

        Assert.Equal(
            4500m,
            request.TotalAmount);
    }
}
