using FlowOps.BuildingBlocks.Domain.Primitives;
using FlowOps.BuildingBlocks.Domain.Results;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.Errors;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;

namespace FlowOps.Modules.Procurement.Domain.PurchaseRequests;

public sealed class PurchaseRequestItem
    : Entity<PurchaseRequestItemId>
{
    private PurchaseRequestItem(
        PurchaseRequestItemId id,
        string description,
        int quantity,
        Money unitPrice)
        : base(id)
    {
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    private PurchaseRequestItem()
    {
        Description = string.Empty;
        UnitPrice = null!;
    }

    public string Description { get; private set; }

    public int Quantity { get; private set; }

    public Money UnitPrice { get; private set; }

    public Money TotalPrice =>
        UnitPrice.Multiply(Quantity);

    public static Result<PurchaseRequestItem> Create(
        string description,
        int quantity,
        Money unitPrice)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Result<PurchaseRequestItem>.Failure(
                PurchaseRequestErrors.DescriptionRequired);
        }

        if (quantity <= 0)
        {
            return Result<PurchaseRequestItem>.Failure(
                PurchaseRequestErrors.InvalidQuantity);
        }

        var item = new PurchaseRequestItem(
            PurchaseRequestItemId.New(),
            description.Trim(),
            quantity,
            unitPrice);

        return Result<PurchaseRequestItem>.Success(item);
    }

    internal Result Update(
        string description,
        int quantity,
        Money unitPrice)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure(
                PurchaseRequestErrors.DescriptionRequired);
        }

        if (quantity <= 0)
        {
            return Result.Failure(
                PurchaseRequestErrors.InvalidQuantity);
        }

        Description = description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;

        return Result.Success();
    }
}