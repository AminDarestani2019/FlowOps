using System.Text.Json.Serialization;

namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

internal sealed class CamundaProcessInstanceSearchResponse
{
    [JsonPropertyName("items")]
    public List<CamundaProcessInstanceDto> Items { get; init; } = [];
}
