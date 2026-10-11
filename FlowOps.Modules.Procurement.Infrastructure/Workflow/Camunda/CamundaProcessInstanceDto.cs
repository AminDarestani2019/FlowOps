using System.Text.Json.Serialization;

namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

internal sealed class CamundaProcessInstanceDto
{
    [JsonPropertyName("processInstanceKey")]
    public string ProcessInstanceKey { get; init; } =
        string.Empty;

    [JsonPropertyName("processDefinitionId")]
    public string ProcessDefinitionId { get; init; } =
        string.Empty;

    [JsonPropertyName("state")]
    public string State { get; init; } =
        string.Empty;

    [JsonPropertyName("businessId")]
    public string? BusinessId { get; init; }
}
