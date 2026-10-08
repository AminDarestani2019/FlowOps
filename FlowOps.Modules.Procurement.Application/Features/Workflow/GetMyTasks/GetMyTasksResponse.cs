namespace FlowOps.Modules.Procurement.Application.Features.Workflow.GetMyTasks;

public sealed record GetMyTasksResponse(
    string TaskKey,
    string Name,
    string State,
    string? Assignee,
    string ProcessDefinitionId,
    string ProcessInstanceKey,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? DueDate,
    int Priority);
