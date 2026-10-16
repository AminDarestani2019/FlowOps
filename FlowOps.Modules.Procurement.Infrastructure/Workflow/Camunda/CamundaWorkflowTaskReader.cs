using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;
using System.Net;
using System.Net.Http.Json;

namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

internal sealed class CamundaWorkflowTaskReader(
    HttpClient httpClient)
    : IWorkflowTaskReader
{
    public async Task<WorkflowTask?> GetActiveTaskForProcessAsync(
    string processInstanceKey,
    CancellationToken cancellationToken = default)
    {
        var request = new
        {
            filter = new
            {
                processInstanceKey,
                state = "CREATED"
            },
            page = new
            {
                limit = 10
            }
        };

        using var response =
            await httpClient.PostAsJsonAsync(
                "/v2/user-tasks/search",
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<CamundaUserTaskSearchResponse>(
                    cancellationToken)
            ?? throw new InvalidOperationException(
                "Camunda returned an empty User Task search response.");

        if (result.Items.Count == 0)
        {
            return null;
        }

        if (result.Items.Count > 1)
        {
            throw new InvalidOperationException(
                $"More than one active User Task was found for " +
                $"process instance '{processInstanceKey}'.");
        }

        return Map(
            result.Items[0]);
    }

    public async Task<IReadOnlyList<WorkflowTask>> GetActiveTasksAsync(
           CancellationToken cancellationToken = default)
    {
        var request = new
        {
            filter = new
            {
                state = "CREATED"
            },
            page = new
            {
                limit = 100
            }
        };

        using var response = await httpClient.PostAsJsonAsync(
            "/v2/user-tasks/search",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<CamundaUserTaskSearchResponse>(
                cancellationToken)
            ?? throw new InvalidOperationException(
                "Camunda returned an empty User Task search response.");

        return result.Items
            .Select(Map)
            .ToList();
    }

    public async Task<WorkflowTask?> GetTaskAsync(
        string taskKey,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"/v2/user-tasks/{taskKey}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var task =
            await response.Content.ReadFromJsonAsync<CamundaUserTaskDto>(
                cancellationToken)
            ?? throw new InvalidOperationException(
                "Camunda returned an empty User Task response.");

        return Map(task);
    }

    private static WorkflowTask Map(
        CamundaUserTaskDto task)
    {
        return new WorkflowTask(
            task.UserTaskKey,
            task.Name,
            task.ElementId,
            task.State,
            task.Assignee,
            task.CandidateGroups,
            task.CandidateUsers,
            task.ProcessDefinitionId,
            task.ProcessInstanceKey,
            task.CreationDate,
            task.DueDate,
            task.Priority);
    }
}