using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;
using System.Net;
using System.Net.Http.Json;

namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

internal sealed class CamundaWorkflowTaskAssigner(
    HttpClient httpClient)
    : IWorkflowTaskAssigner
{
    public async Task<WorkflowTaskAssignmentOutcome> ClaimAsync(
        string taskKey,
        string assignee,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            assignee,
            allowOverride = false,
            action = "claim"
        };

        using var response =
            await httpClient.PostAsJsonAsync(
                $"/v2/user-tasks/{taskKey}/assignment",
                payload,
                cancellationToken);

        return Map(response);
    }

    public async Task<WorkflowTaskAssignmentOutcome> UnclaimAsync(
        string taskKey,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await httpClient.DeleteAsync(
                $"/v2/user-tasks/{taskKey}/assignee",
                cancellationToken);

        return Map(response);
    }

    private static WorkflowTaskAssignmentOutcome Map(
        HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return WorkflowTaskAssignmentOutcome.Success;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return WorkflowTaskAssignmentOutcome.NotFound;
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return WorkflowTaskAssignmentOutcome.Conflict;
        }

        response.EnsureSuccessStatusCode();

        return WorkflowTaskAssignmentOutcome.Success;
    }
}
