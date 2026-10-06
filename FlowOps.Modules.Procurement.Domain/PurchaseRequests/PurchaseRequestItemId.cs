namespace FlowOps.Modules.Procurement.Domain.PurchaseRequests;

public readonly record struct PurchaseRequestItemId(Guid Value)
{
    public static PurchaseRequestItemId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}