using FlowOps.BuildingBlocks.Domain.Results;

namespace FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;

public sealed record Money
{
    private Money()
    {
        Currency = null!;
    }

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; private set; }

    public Currency Currency { get; private set; }

    public static Result<Money> Create(
        decimal amount,
        Currency currency)
    {
        if (amount < 0)
        {
            return Result<Money>.Failure(
                new Error(
                    "Money.NegativeAmount",
                    "Money amount cannot be negative."));
        }

        return Result<Money>.Success(
            new Money(amount, currency));
    }

    public Money Multiply(int quantity)
    {
        return new Money(
            Amount * quantity,
            Currency);
    }
}
