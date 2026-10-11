using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

namespace FlowOps.Modules.Procurement.Application.Features.Workflow.GetMyTasks;

public sealed class Handler(
    IWorkflowTaskReader workflowTaskReader)
{
    public async Task<IReadOnlyList<Response>> HandleAsync(
        Query query,
        CancellationToken cancellationToken = default)
    {
        var tasks = await workflowTaskReader.GetActiveTasksAsync(
            cancellationToken);

        return tasks
            .Select(task => new Response(
                task.TaskKey,
                task.Name,
                task.State,
                task.Assignee,
                task.ProcessDefinitionId,
                task.ProcessInstanceKey,
                task.CreatedAt,
                task.DueDate,
                task.Priority))
            .ToList();
    }
}