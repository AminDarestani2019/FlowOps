namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.CreatePurchaseRequest;

public sealed record CreatePurchaseRequestResponse(
    Guid Id,
    string RequestNumber,
    string Status);
