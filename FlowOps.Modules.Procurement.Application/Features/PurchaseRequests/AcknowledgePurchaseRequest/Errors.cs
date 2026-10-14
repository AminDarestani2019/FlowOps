using FlowOps.BuildingBlocks.Domain.Results;

namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.AcknowledgePurchaseRequest;

public static class Errors
{
    public static readonly Error NotFound =
        new(
            "PurchaseRequest.NotFound",
            "Purchase request was not found.");

    public static readonly Error WorkflowMessageNotCorrelated =
        new(
            "Workflow.MessageNotCorrelated",
            "No active workflow message subscription was found for the specified purchase request.");
}