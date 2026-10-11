namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.RejectPurchaseRequest;

public sealed record RejectPurchaseRequestCommand(
    Guid PurchaseRequestId,
    string TaskKey);