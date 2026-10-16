namespace FlowOps.Modules.Procurement.Application.Features.Workflow.GetTaskDetails;

public sealed record Response(
    string TaskKey,
    string Name,
    string State,
    string? Assignee,
    string ProcessDefinitionId,
    string ProcessInstanceKey,
    string? BusinessId,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? DueDate,
    string? SlaStatus,
    int Priority);
