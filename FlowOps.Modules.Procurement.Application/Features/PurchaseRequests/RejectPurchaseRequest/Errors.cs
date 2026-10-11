using FlowOps.BuildingBlocks.Domain.Results;

namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.RejectPurchaseRequest;

internal static class Errors
{
    public static readonly Error NotFound =
        new(
            "PurchaseRequest.NotFound",
            "Purchase request was not found.");

    public static readonly Error WorkflowTaskNotFound =
        new(
            "Workflow.TaskNotFound",
            "Workflow task was not found.");

    public static readonly Error WorkflowProcessNotFound =
        new(
            "Workflow.ProcessNotFound",
            "Workflow process instance was not found.");

    public static readonly Error WorkflowCorrelationMismatch =
        new(
            "Workflow.CorrelationMismatch",
            "Workflow process is not correlated with the purchase request.");

    public static readonly Error WorkflowTaskNotActive =
        new(
            "Workflow.TaskNotActive",
            "Workflow task is not active.");
}