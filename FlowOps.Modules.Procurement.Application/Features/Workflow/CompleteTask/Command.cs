namespace FlowOps.Modules.Procurement.Application.Features.Workflow.CompleteTask;

public sealed record Command(
    string TaskKey,
    string? Action,
    IReadOnlyDictionary<string, object?>? Variables);
