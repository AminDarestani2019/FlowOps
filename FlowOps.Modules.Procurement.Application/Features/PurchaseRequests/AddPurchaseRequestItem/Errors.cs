using FlowOps.BuildingBlocks.Domain.Results;

namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.AddPurchaseRequestItem;

public static class AddPurchaseRequestItemErrors
{
    public static readonly Error PurchaseRequestNotFound =
        new(
            "PurchaseRequest.NotFound",
            "The purchase request was not found.");
}