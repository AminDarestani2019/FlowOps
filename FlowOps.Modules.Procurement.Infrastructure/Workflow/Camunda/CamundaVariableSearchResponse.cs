using System.Text.Json.Serialization;

namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

internal sealed class CamundaVariableSearchResponse
{
    [JsonPropertyName("items")]
    public List<CamundaVariableDto> Items { get; init; } = [];
}

internal sealed class CamundaVariableDto
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; init; } = string.Empty;

    [JsonPropertyName("processInstanceKey")]
    public string ProcessInstanceKey { get; init; } = string.Empty;
}