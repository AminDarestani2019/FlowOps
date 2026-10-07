namespace FlowOps.Modules.Procurement.Application.Queries.Models;

public sealed record PurchaseRequestListItem(
    Guid Id,
    string RequestNumber,
    string Status,
    string Currency,
    decimal TotalAmount,
    int ItemCount,
    DateTime CreatedAtUtc);
