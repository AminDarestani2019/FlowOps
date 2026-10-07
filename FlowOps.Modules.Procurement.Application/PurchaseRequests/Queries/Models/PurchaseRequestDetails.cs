namespace FlowOps.Modules.Procurement.Application.PurchaseRequests.Queries.Models;

public sealed record PurchaseRequestDetails(
    Guid Id,
    string RequestNumber,
    Guid RequesterId,
    Guid DepartmentId,
    string Currency,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? SubmittedAtUtc,
    decimal TotalAmount,
    IReadOnlyList<PurchaseRequestItemDetails> Items);
