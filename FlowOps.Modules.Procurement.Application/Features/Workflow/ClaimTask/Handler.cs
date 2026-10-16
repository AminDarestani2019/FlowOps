using FlowOps.Modules.Procurement.Application.Abstractions.Identity;
using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

namespace FlowOps.Modules.Procurement.Application.Features.Workflow.ClaimTask;

public sealed class Handler(
    IWorkflowTaskReader workflowTaskReader,
    IWorkflowTaskAssigner workflowTaskAssigner,
    IWorkflowCurrentUser currentUser)
{
    public async Task<Result> HandleAsync(
        Command command,
        CancellationToken cancellationToken = default)
    {
        var task =
            await workflowTaskReader.GetTaskAsync(
                command.TaskKey,
                cancellationToken);

        if (task is null)
        {
            return new Result(
                ClaimTaskStatus.NotFound);
        }

        if (!string.Equals(
                task.State,
                "CREATED",
                StringComparison.Ordinal))
        {
            return new Result(
                ClaimTaskStatus.Conflict);
        }

        if (!string.IsNullOrWhiteSpace(task.Assignee))
        {
            return new Result(
                ClaimTaskStatus.Conflict);
        }

        if (!WorkflowTaskVisibilityPolicy.IsVisibleTo(
                task,
                currentUser.UserId,
                currentUser.CandidateGroups))
        {
            return new Result(
                ClaimTaskStatus.Forbidden);
        }

        var outcome =
            await workflowTaskAssigner.ClaimAsync(
                command.TaskKey,
                currentUser.UserId,
                cancellationToken);

        return outcome switch
        {
            WorkflowTaskAssignmentOutcome.Success =>
                new Result(
                    ClaimTaskStatus.Claimed),

            WorkflowTaskAssignmentOutcome.NotFound =>
                new Result(
                    ClaimTaskStatus.NotFound),

            WorkflowTaskAssignmentOutcome.Conflict =>
                new Result(
                    ClaimTaskStatus.Conflict),

            _ => throw new InvalidOperationException(
                "Unsupported workflow task assignment outcome.")
        };
    }
}
