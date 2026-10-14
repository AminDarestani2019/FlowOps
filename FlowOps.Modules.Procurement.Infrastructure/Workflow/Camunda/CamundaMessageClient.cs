using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;
using System.Net;

namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

internal sealed class CamundaMessageClient(HttpClient httpClient): IWorkflowMessageCorrelator
{
    public async Task<WorkflowMessageCorrelationResult?> CorrelateAsync(
        string messageName,
        string correlationKey,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageName);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationKey);

        var request = new CamundaMessageCorrelationRequest
        {
            Name = messageName,
            CorrelationKey = correlationKey
        };

        using var response = await httpClient.PostAsJsonAsync(
            "/v2/messages/correlation",
            request,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<CamundaMessageCorrelationResponse>(
                cancellationToken: cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException(
                "Camunda returned an empty message correlation response.");
        }

        return new WorkflowMessageCorrelationResult(
            result.MessageKey,
            result.ProcessInstanceKey,
            result.TenantId);
    }
}

internal sealed class CamundaMessageCorrelationRequest
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("correlationKey")]
    public string CorrelationKey { get; init; } = string.Empty;
}

internal sealed class CamundaMessageCorrelationResponse
{
    [JsonPropertyName("messageKey")]
    public string MessageKey { get; init; } = string.Empty;

    [JsonPropertyName("processInstanceKey")]
    public string ProcessInstanceKey { get; init; } = string.Empty;

    [JsonPropertyName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
}