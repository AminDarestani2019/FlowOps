namespace FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

public sealed record WorkflowProcessInstance(
    string ProcessInstanceKey,
    string ProcessDefinitionId,
    string State,
    string? BusinessId);
