namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.AcknowledgePurchaseRequest;

public sealed record AcknowledgePurchaseRequestResponse(
    Guid PurchaseRequestId,
    string MessageKey,
    string ProcessInstanceKey);