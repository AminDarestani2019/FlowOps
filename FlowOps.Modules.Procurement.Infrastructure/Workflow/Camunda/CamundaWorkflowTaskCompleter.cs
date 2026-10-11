using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;
using System.Net;
using System.Net.Http.Json;

namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

internal sealed class CamundaWorkflowTaskCompleter(
    HttpClient httpClient)
    : IWorkflowTaskCompleter
{
    public async Task<bool> CompleteAsync(
        string taskKey,
        CompleteWorkflowTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            action = request.Action,
            variables = request.Variables
        };

        using var response = await httpClient.PostAsJsonAsync(
            $"/v2/user-tasks/{taskKey}/completion",
            payload,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();

        return true;
    }
}
