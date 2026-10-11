namespace FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

public interface IWorkflowTaskCompleter
{
    Task<bool> CompleteAsync(
        string taskKey,
        CompleteWorkflowTaskRequest request,
        CancellationToken cancellationToken = default);
}
