namespace FlowOps.Modules.Procurement.Application.Features.Workflow.GetMyTasks;

public sealed record Response(
    string TaskKey,
    string Name,
    string ElementId,
    string State,
    string? Assignee,
    IReadOnlyList<string> CandidateGroups,
    IReadOnlyList<string> CandidateUsers,
    string ProcessDefinitionId,
    string ProcessInstanceKey,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? DueDate,
    string? SlaStatus,
    int Priority);
