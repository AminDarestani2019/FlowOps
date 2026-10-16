namespace FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

public interface IWorkflowTaskAssigner
{
    Task<WorkflowTaskAssignmentOutcome> ClaimAsync(
        string taskKey,
        string assignee,
        CancellationToken cancellationToken = default);

    Task<WorkflowTaskAssignmentOutcome> UnclaimAsync(
        string taskKey,
        CancellationToken cancellationToken = default);
}
