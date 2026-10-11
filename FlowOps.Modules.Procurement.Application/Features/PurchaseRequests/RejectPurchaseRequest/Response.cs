namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.RejectPurchaseRequest;

public sealed record RejectPurchaseRequestResponse(
    Guid PurchaseRequestId,
    string Status);