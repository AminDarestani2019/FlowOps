using FlowOps.Modules.Procurement.Application.Abstractions.Identity;
using FlowOps.Modules.Procurement.Application.Abstractions.Persistence;
using FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.Models;

namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.GetMyPurchaseRequests;

public sealed class GetMyPurchaseRequestsHandler
{
    private readonly IPurchaseRequestReadService _readService;
    private readonly ICurrentUser _currentUser;

    public GetMyPurchaseRequestsHandler(
        IPurchaseRequestReadService readService,
        ICurrentUser currentUser)
    {
        _readService = readService;
        _currentUser = currentUser;
    }

    public Task<PagedResult<PurchaseRequestListItem>> HandleAsync(
        GetMyPurchaseRequestsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page =
            Math.Max(query.Page, 1);

        var pageSize =
            Math.Clamp(
                query.PageSize,
                1,
                100);

        return _readService.GetByRequesterAsync(
            _currentUser.EmployeeId,
            query.Search,
            query.Status,
            query.SortBy,
            query.Descending,
            page,
            pageSize,
            cancellationToken);
    }
}