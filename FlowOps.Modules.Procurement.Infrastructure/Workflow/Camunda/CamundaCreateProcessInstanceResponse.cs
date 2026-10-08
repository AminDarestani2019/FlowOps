using System.Text.Json.Serialization;

namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

internal sealed record CamundaCreateProcessInstanceResponse(
    [property: JsonPropertyName("processDefinitionId")]
    string ProcessDefinitionId,

    [property: JsonPropertyName("processDefinitionVersion")]
    int ProcessDefinitionVersion,

    [property: JsonPropertyName("processDefinitionKey")]
    string ProcessDefinitionKey,

    [property: JsonPropertyName("processInstanceKey")]
    string ProcessInstanceKey,

    [property: JsonPropertyName("businessId")]
    string? BusinessId);