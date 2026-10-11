using System.Text.Json.Serialization;

namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

internal sealed class CamundaUserTaskSearchResponse
{
    [JsonPropertyName("items")]
    public List<CamundaUserTaskDto> Items { get; init; } = [];
}

internal sealed class CamundaUserTaskDto
{
    [JsonPropertyName("userTaskKey")]
    public string UserTaskKey { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("elementId")]
    public string ElementId { get; init; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; init; } = string.Empty;

    [JsonPropertyName("assignee")]
    public string? Assignee { get; init; }

    [JsonPropertyName("processDefinitionId")]
    public string ProcessDefinitionId { get; init; } = string.Empty;

    [JsonPropertyName("processInstanceKey")]
    public string ProcessInstanceKey { get; init; } = string.Empty;

    [JsonPropertyName("creationDate")]
    public DateTimeOffset? CreationDate { get; init; }

    [JsonPropertyName("dueDate")]
    public DateTimeOffset? DueDate { get; init; }

    [JsonPropertyName("priority")]
    public int Priority { get; init; }
}
