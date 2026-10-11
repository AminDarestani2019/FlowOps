using FlowOps.BuildingBlocks.Domain.Results;

namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.GetPurchaseRequest;

public static class GetPurchaseRequestErrors
{
    public static readonly Error NotFound =
        new(
            "PurchaseRequest.NotFound",
            "The purchase request was not found.");
}
