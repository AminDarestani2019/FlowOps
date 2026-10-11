namespace FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

public interface IWorkflowProcessReader
{
    Task<WorkflowProcessInstance?> GetByKeyAsync(
        string processInstanceKey,
        CancellationToken cancellationToken = default);

    Task<WorkflowProcessInstance?> GetActiveByBusinessIdAsync(
        string businessId,
        CancellationToken cancellationToken = default);
}