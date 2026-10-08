namespace FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

public sealed record WorkflowTask(
    string TaskKey,
    string Name,
    string State,
    string? Assignee,
    string ProcessDefinitionId,
    string ProcessInstanceKey,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? DueDate,
    int Priority);