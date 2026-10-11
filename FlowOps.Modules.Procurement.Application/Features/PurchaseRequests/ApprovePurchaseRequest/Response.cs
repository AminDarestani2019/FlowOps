namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.ApprovePurchaseRequest;

public sealed record ApprovePurchaseRequestResponse(
    Guid PurchaseRequestId,
    string Status);
