using FlowOps.BuildingBlocks.Domain.Results;

namespace FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;

public sealed record PurchaseRequestNumber
{
    private PurchaseRequestNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PurchaseRequestNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<PurchaseRequestNumber>.Failure(
                new Error(
                    "PurchaseRequest.NumberRequired",
                    "Purchase request number is required."));
        }

        return Result<PurchaseRequestNumber>.Success(
            new PurchaseRequestNumber(value.Trim()));
    }

    public override string ToString() => Value;
}