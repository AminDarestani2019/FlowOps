namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.AddPurchaseRequestItem;

public sealed record AddPurchaseRequestItemCommand(
    Guid PurchaseRequestId,
    string Description,
    int Quantity,
    decimal UnitPrice);