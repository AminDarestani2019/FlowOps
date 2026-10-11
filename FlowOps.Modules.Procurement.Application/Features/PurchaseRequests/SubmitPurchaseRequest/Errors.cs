using FlowOps.BuildingBlocks.Domain.Results;

namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.SubmitPurchaseRequest;

public static class SubmitPurchaseRequestErrors
{
    public static readonly Error NotFound =
        new(
            "PurchaseRequest.NotFound",
            "The purchase request was not found.");
}
