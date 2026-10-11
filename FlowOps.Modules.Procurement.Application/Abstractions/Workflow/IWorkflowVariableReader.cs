namespace FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

public interface IWorkflowVariableReader
{
    Task<WorkflowVariable?> GetAsync(
        string processInstanceKey,
        string variableName,
        CancellationToken cancellationToken = default);
}