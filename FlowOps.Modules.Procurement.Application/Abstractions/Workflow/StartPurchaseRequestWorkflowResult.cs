namespace FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

public sealed record StartPurchaseRequestWorkflowResult(
    string ProcessInstanceKey,
    string ProcessDefinitionKey,
    int ProcessDefinitionVersion,
    string BusinessId);