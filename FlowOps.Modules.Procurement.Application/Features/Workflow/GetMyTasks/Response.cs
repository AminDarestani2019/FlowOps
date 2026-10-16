namespace FlowOps.Modules.Procurement.Application.Features.Workflow.GetMyTasks;

public sealed record Response(
    string TaskKey,
    string Name,
    string State,
    string? Assignee,
    string ProcessDefinitionId,
    string ProcessInstanceKey,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? DueDate,
    string? SlaStatus,
    int Priority);
