namespace FlowOps.Modules.Procurement.Application.PurchaseRequests.Queries.GetPurchaseRequests;

public sealed record GetPurchaseRequestsQuery(
    string? Search,
    string? Status,
    string SortBy = "createdAt",
    bool Descending = true,
    int Page = 1,
    int PageSize = 20);
