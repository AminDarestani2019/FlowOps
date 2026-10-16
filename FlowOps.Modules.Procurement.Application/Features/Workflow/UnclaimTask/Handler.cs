using FlowOps.Modules.Procurement.Application.Abstractions.Identity;
using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

namespace FlowOps.Modules.Procurement.Application.Features.Workflow.UnclaimTask;

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
                UnclaimTaskStatus.NotFound);
        }

        if (!string.Equals(
                task.State,
                "CREATED",
                StringComparison.Ordinal))
        {
            return new Result(
                UnclaimTaskStatus.Conflict);
        }

        if (string.IsNullOrWhiteSpace(task.Assignee))
        {
            return new Result(
                UnclaimTaskStatus.Conflict);
        }

        if (!string.Equals(
                task.Assignee,
                currentUser.UserId,
                StringComparison.Ordinal))
        {
            return new Result(
                UnclaimTaskStatus.Forbidden);
        }

        var outcome =
            await workflowTaskAssigner.UnclaimAsync(
                command.TaskKey,
                cancellationToken);

        return outcome switch
        {
            WorkflowTaskAssignmentOutcome.Success =>
                new Result(
                    UnclaimTaskStatus.Unclaimed),

            WorkflowTaskAssignmentOutcome.NotFound =>
                new Result(
                    UnclaimTaskStatus.NotFound),

            WorkflowTaskAssignmentOutcome.Conflict =>
                new Result(
                    UnclaimTaskStatus.Conflict),

            _ => throw new InvalidOperationException(
                "Unsupported workflow task assignment outcome.")
        };
    }
}
