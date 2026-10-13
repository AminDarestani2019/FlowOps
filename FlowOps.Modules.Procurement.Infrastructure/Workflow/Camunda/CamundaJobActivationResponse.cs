using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

internal sealed class CamundaJobActivationResponse
{
    [JsonPropertyName("jobs")]
    public List<CamundaActivatedJobDto> Jobs { get; init; } = [];
}

internal sealed class CamundaActivatedJobDto
{
    [JsonPropertyName("jobKey")]
    public string JobKey { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("processInstanceKey")]
    public string ProcessInstanceKey { get; init; } = string.Empty;

    [JsonPropertyName("processDefinitionKey")]
    public string ProcessDefinitionKey { get; init; } = string.Empty;

    [JsonPropertyName("elementInstanceKey")]
    public string ElementInstanceKey { get; init; } = string.Empty;

    [JsonPropertyName("elementId")]
    public string ElementId { get; init; } = string.Empty;

    [JsonPropertyName("retries")]
    public int Retries { get; init; }

    [JsonPropertyName("variables")]
    public Dictionary<string, JsonElement> Variables { get; init; } = [];
}