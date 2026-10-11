namespace FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

public interface IWorkflowTaskReader
{
    Task<IReadOnlyList<WorkflowTask>> GetActiveTasksAsync(
        CancellationToken cancellationToken = default);

    Task<WorkflowTask?> GetTaskAsync(
        string taskKey,
        CancellationToken cancellationToken = default);

    Task<WorkflowTask?> GetActiveTaskForProcessAsync(
        string processInstanceKey,
        CancellationToken cancellationToken = default);
}
