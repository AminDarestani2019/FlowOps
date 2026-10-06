namespace FlowOps.Modules.Procurement.Application
    .PurchaseRequests.AddPurchaseRequestItem;

public sealed record AddPurchaseRequestItemCommand(
    Guid PurchaseRequestId,
    string Description,
    int Quantity,
    decimal UnitPrice);