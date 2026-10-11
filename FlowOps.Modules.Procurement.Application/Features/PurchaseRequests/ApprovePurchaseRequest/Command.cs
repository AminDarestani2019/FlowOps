namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.ApprovePurchaseRequest;

public sealed record ApprovePurchaseRequestCommand(
    Guid PurchaseRequestId,
    string TaskKey);