using FlowOps.BuildingBlocks.Domain.Results;

namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.ApprovePurchaseRequest;

public static class Errors
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
            "The workflow task does not belong to the specified purchase request.");

    public static readonly Error WorkflowTaskNotActive =
        new(
            "Workflow.TaskNotActive",
            "The workflow task is not active.");

    public static readonly Error WorkflowApprovalLevelNotFound =
        new(
            "Workflow.ApprovalLevelNotFound",
            "The workflow approval level was not found.");

    public static readonly Error WorkflowApprovalContextInvalid =
        new(
            "Workflow.ApprovalContextInvalid",
            "The workflow approval stage is not valid for the configured approval level.");
}
