using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

namespace FlowOps.Modules.Procurement.Application.Features.Workflow.CompleteTask;

public sealed class Handler(
    IWorkflowTaskCompleter workflowTaskCompleter)
{
    public async Task<Result> HandleAsync(
        Command command,
        CancellationToken cancellationToken = default)
    {
        var completed = await workflowTaskCompleter.CompleteAsync(
            command.TaskKey,
            new CompleteWorkflowTaskRequest(
                command.Action,
                command.Variables),
            cancellationToken);

        return new Result(completed);
    }
}
