using FlowOps.BuildingBlocks.Domain.Time;
using FlowOps.Modules.Procurement.Application.Abstractions.Identity;
using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

namespace FlowOps.Modules.Procurement.Application.Features.Workflow.GetTaskDetails;

public sealed class Handler(
    IWorkflowTaskReader workflowTaskReader,
    IWorkflowProcessReader workflowProcessReader,
    IWorkflowVariableReader workflowVariableReader,
    IWorkflowCurrentUser currentUser,
    IClock clock)
{
    private const string ManagerApprovalElementId =
        "UserTask_ManagerApproval";

    private const string ManagerApprovalEscalatedVariable =
        "managerApprovalEscalated";

    public async Task<Response?> HandleAsync(
        Query query,
        CancellationToken cancellationToken = default)
    {
        var task =
            await workflowTaskReader.GetTaskAsync(
                query.TaskKey,
                cancellationToken);

        if (task is null)
        {
            return null;
        }

        if (!WorkflowTaskVisibilityPolicy.IsVisibleTo(
                task,
                currentUser.UserId,
                currentUser.CandidateGroups))
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

        var managerApprovalEscalated =
            await IsManagerApprovalEscalatedAsync(
                task,
                cancellationToken);

        var slaStatus =
            WorkflowTaskSlaStatusResolver.Resolve(
                task,
                clock.UtcNow,
                managerApprovalEscalated);

        return new Response(
            task.TaskKey,
            task.Name,
            task.ElementId,
            task.State,
            task.Assignee,
            task.CandidateGroups,
            task.CandidateUsers,
            task.ProcessDefinitionId,
            task.ProcessInstanceKey,
            processInstance.BusinessId,
            task.CreatedAt,
            task.DueDate,
            slaStatus,
            task.Priority);
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
