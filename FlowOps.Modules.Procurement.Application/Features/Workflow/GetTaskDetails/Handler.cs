using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

namespace FlowOps.Modules.Procurement.Application.Features.Workflow.GetTaskDetails;

public sealed class Handler(
    IWorkflowTaskReader workflowTaskReader,
    IWorkflowProcessReader workflowProcessReader)
{
    public async Task<Response?> HandleAsync(
        Query query,
        CancellationToken cancellationToken = default)
    {
        var task = await workflowTaskReader.GetTaskAsync(
            query.TaskKey,
            cancellationToken);

        if (task is null)
        {
            return null;
        }

        var processInstance =
            await workflowProcessReader.GetByKeyAsync(
                task.ProcessInstanceKey,
                cancellationToken);

        if (processInstance is null)
        {
            return null;
        }

        return new Response(
            task.TaskKey,
            task.Name,
            task.State,
            task.Assignee,
            task.ProcessDefinitionId,
            task.ProcessInstanceKey,
            processInstance.BusinessId,
            task.CreatedAt,
            task.DueDate,
            task.Priority);
    }
}