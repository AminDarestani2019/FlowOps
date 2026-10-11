namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.SubmitPurchaseRequest;

public sealed record SubmitPurchaseRequestResponse(
    Guid Id,
    string RequestNumber,
    string Status,
    DateTime SubmittedAtUtc);