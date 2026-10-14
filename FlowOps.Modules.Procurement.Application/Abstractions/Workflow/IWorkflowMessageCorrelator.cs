namespace FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

public interface IWorkflowMessageCorrelator
{
    Task<WorkflowMessageCorrelationResult?> CorrelateAsync(
        string messageName,
        string correlationKey,
        CancellationToken cancellationToken);
}

public sealed record WorkflowMessageCorrelationResult(
    string MessageKey,
    string ProcessInstanceKey,
    string TenantId);