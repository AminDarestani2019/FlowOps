using FlowOps.BuildingBlocks.Domain.Results;

namespace FlowOps.Modules.Procurement.Application.PurchaseRequests.SubmitPurchaseRequest;

public static class SubmitPurchaseRequestErrors
{
    public static readonly Error NotFound =
        new(
            "PurchaseRequest.NotFound",
            "The purchase request was not found.");
}
