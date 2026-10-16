using FlowOps.BuildingBlocks.Domain.Time;
using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

namespace FlowOps.Modules.Procurement.Application.Features.Workflow.GetMyTasks;

public sealed class Handler(
    IWorkflowTaskReader workflowTaskReader,
    IWorkflowVariableReader workflowVariableReader,
    IClock clock)
{
    private const string ManagerApprovalElementId =
        "UserTask_ManagerApproval";

    private const string ManagerApprovalEscalatedVariable =
        "managerApprovalEscalated";

    public async Task<IReadOnlyList<Response>> HandleAsync(
        Query query,
        CancellationToken cancellationToken = default)
    {
        var tasks =
            await workflowTaskReader.GetActiveTasksAsync(
                cancellationToken);

        var responses =
            new List<Response>(tasks.Count);

        foreach (var task in tasks)
        {
            var managerApprovalEscalated =
                await IsManagerApprovalEscalatedAsync(
                    task,
                    cancellationToken);

            var slaStatus =
                WorkflowTaskSlaStatusResolver.Resolve(
                    task,
                    clock.UtcNow,
                    managerApprovalEscalated);

            responses.Add(
                new Response(
                    task.TaskKey,
                    task.Name,
                    task.State,
                    task.Assignee,
                    task.ProcessDefinitionId,
                    task.ProcessInstanceKey,
                    task.CreatedAt,
                    task.DueDate,
                    slaStatus,
                    task.Priority));
        }

        return responses;
    }

    private async Task<bool>
        IsManagerApprovalEscalatedAsync(
            WorkflowTask task,
            CancellationToken cancellationToken)
    {
        if (!string.Equals(
                task.ElementId,
                ManagerApprovalElementId,
                StringComparison.Ordinal))
        {
            return false;
        }

        var variable =
            await workflowVariableReader.GetAsync(
                task.ProcessInstanceKey,
                ManagerApprovalEscalatedVariable,
                cancellationToken);

        if (variable is null)
        {
            return false;
        }

        return string.Equals(
            variable.Value,
            "true",
            StringComparison.OrdinalIgnoreCase);
    }
}
