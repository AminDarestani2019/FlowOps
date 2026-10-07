namespace FlowOps.Modules.Procurement.Application.Queries.GetMyPurchaseRequests;

public sealed record GetMyPurchaseRequestsQuery(
    string? Search,
    string? Status,
    string SortBy = "createdAt",
    bool Descending = true,
    int Page = 1,
    int PageSize = 20);
