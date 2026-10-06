namespace FlowOps.Modules.Procurement.Application
    .PurchaseRequests.AddPurchaseRequestItem;

public sealed record AddPurchaseRequestItemResponse(
    Guid ItemId,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    decimal RequestTotal,
    string Currency);