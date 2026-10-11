namespace FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

public sealed record CompleteWorkflowTaskRequest(
    string? Action,
    IReadOnlyDictionary<string, object?>? Variables);
