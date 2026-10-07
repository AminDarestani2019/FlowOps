using FlowOps.BuildingBlocks.Domain.Results;
using FlowOps.Modules.Procurement.Application.Abstractions.Persistence;
using FlowOps.Modules.Procurement.Application.PurchaseRequests.Queries.Models;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests;

namespace FlowOps.Modules.Procurement.Application.PurchaseRequests.Queries.GetPurchaseRequest;

public sealed class GetPurchaseRequestHandler
{
    private readonly IPurchaseRequestReadService _readService;

    public GetPurchaseRequestHandler(
        IPurchaseRequestReadService readService)
    {
        _readService = readService;
    }

    public async Task<Result<PurchaseRequestDetails>> HandleAsync(
        GetPurchaseRequestQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.PurchaseRequestId == Guid.Empty)
        {
            return Result<PurchaseRequestDetails>.Failure(
                GetPurchaseRequestErrors.NotFound);
        }

        var purchaseRequest =
            await _readService.GetByIdAsync(
                new PurchaseRequestId(
                    query.PurchaseRequestId),
                cancellationToken);

        if (purchaseRequest is null)
        {
            return Result<PurchaseRequestDetails>.Failure(
                GetPurchaseRequestErrors.NotFound);
        }

        return Result<PurchaseRequestDetails>.Success(
            purchaseRequest);
    }
}
