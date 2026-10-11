using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;
using System.Net.Http.Json;

namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

internal sealed class CamundaWorkflowVariableReader(
    HttpClient httpClient)
    : IWorkflowVariableReader
{
    public async Task<WorkflowVariable?> GetAsync(
        string processInstanceKey,
        string variableName,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            filter = new
            {
                processInstanceKey
            },
            page = new
            {
                limit = 100
            }
        };

        using var response =
            await httpClient.PostAsJsonAsync(
                "/v2/variables/search",
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<CamundaVariableSearchResponse>(
                    cancellationToken)
            ?? throw new InvalidOperationException(
                "Camunda returned an empty Variable search response.");

        var matches =
            result.Items
                .Where(x =>
                    string.Equals(
                        x.Name,
                        variableName,
                        StringComparison.Ordinal))
                .ToList();

        if (matches.Count == 0)
        {
            return null;
        }

        if (matches.Count > 1)
        {
            throw new InvalidOperationException(
                $"More than one workflow variable named " +
                $"'{variableName}' was found for process instance " +
                $"'{processInstanceKey}'.");
        }

        var variable = matches[0];

        return new WorkflowVariable(
            variable.Name,
            variable.Value);
    }
}