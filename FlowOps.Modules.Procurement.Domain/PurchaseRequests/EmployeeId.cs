namespace FlowOps.Modules.Procurement.Domain.PurchaseRequests;

public readonly record struct EmployeeId(Guid Value)
{
    public static EmployeeId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
