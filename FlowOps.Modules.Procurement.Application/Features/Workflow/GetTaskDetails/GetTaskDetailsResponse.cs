namespace FlowOps.Modules.Procurement.Application.Features.Workflow.GetTaskDetails;

public sealed record GetTaskDetailsResponse(
    string TaskKey,
    string Name,
    string State,
    string? Assignee,
    string ProcessDefinitionId,
    string ProcessInstanceKey,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? DueDate,
    int Priority);