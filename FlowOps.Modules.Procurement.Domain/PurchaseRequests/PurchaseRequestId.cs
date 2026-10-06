namespace FlowOps.Modules.Procurement.Domain.PurchaseRequests;

public readonly record struct PurchaseRequestId(Guid Value)
{
    public static PurchaseRequestId New()
    {
        return new PurchaseRequestId(Guid.NewGuid());
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
