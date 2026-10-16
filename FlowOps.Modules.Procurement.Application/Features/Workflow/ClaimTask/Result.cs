namespace FlowOps.Modules.Procurement.Application.Features.Workflow.ClaimTask;

public enum ClaimTaskStatus
{
    Claimed,
    NotFound,
    Forbidden,
    Conflict
}

public sealed record Result(
    ClaimTaskStatus Status);
