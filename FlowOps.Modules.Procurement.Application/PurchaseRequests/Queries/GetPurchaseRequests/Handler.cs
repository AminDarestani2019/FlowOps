using FlowOps.Modules.Procurement.Application.Abstractions.Persistence;
using FlowOps.Modules.Procurement.Application.PurchaseRequests.Queries.Models;

namespace FlowOps.Modules.Procurement.Application.PurchaseRequests.Queries.GetPurchaseRequests;

public sealed class GetPurchaseRequestsHandler
{
    private readonly IPurchaseRequestReadService _readService;

    public GetPurchaseRequestsHandler(
        IPurchaseRequestReadService readService)
    {
        _readService = readService;
    }

    public Task<PagedResult<PurchaseRequestListItem>> HandleAsync(
        GetPurchaseRequestsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page =
            Math.Max(query.Page, 1);

        var pageSize =
            Math.Clamp(
                query.PageSize,
                1,
                100);

        return _readService.GetAsync(
            query.Search,
            query.Status,
            query.SortBy,
            query.Descending,
            page,
            pageSize,
            cancellationToken);
    }
}
