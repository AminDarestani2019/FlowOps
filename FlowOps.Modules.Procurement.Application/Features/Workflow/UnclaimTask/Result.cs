namespace FlowOps.Modules.Procurement.Application.Features.Workflow.UnclaimTask;

public enum UnclaimTaskStatus
{
    Unclaimed,
    NotFound,
    Forbidden,
    Conflict
}

public sealed record Result(
    UnclaimTaskStatus Status);
