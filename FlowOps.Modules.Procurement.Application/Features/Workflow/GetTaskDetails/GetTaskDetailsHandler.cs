using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

namespace FlowOps.Modules.Procurement.Application.Features.Workflow.GetTaskDetails;

public sealed class GetTaskDetailsHandler(
    IWorkflowTaskReader workflowTaskReader)
{
    public async Task<GetTaskDetailsResponse?> HandleAsync(
        GetTaskDetailsQuery query,
        CancellationToken cancellationToken = default)
    {
        var task = await workflowTaskReader.GetTaskAsync(
            query.TaskKey,
            cancellationToken);

        if (task is null)
        {
            return null;
        }

        return new GetTaskDetailsResponse(
            task.TaskKey,
            task.Name,
            task.State,
            task.Assignee,
            task.ProcessDefinitionId,
            task.ProcessInstanceKey,
            task.CreatedAt,
            task.DueDate,
            task.Priority);
    }
}