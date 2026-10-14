namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.AcknowledgePurchaseRequest;

public sealed record AcknowledgePurchaseRequestCommand(
    Guid PurchaseRequestId);