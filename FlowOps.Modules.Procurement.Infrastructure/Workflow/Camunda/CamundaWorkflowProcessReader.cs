using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;
using System.Net;
using System.Net.Http.Json;

namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

internal sealed class CamundaWorkflowProcessReader(
    HttpClient httpClient)
    : IWorkflowProcessReader
{
    public async Task<WorkflowProcessInstance?> GetByKeyAsync(
        string processInstanceKey,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await httpClient.GetAsync(
                $"/v2/process-instances/{processInstanceKey}",
                cancellationToken);

        if (response.StatusCode ==
            HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var processInstance =
            await response.Content
                .ReadFromJsonAsync<CamundaProcessInstanceDto>(
                    cancellationToken)
            ?? throw new InvalidOperationException(
                "Camunda returned an empty process instance response.");

        return new WorkflowProcessInstance(
            processInstance.ProcessInstanceKey,
            processInstance.ProcessDefinitionId,
            processInstance.State,
            processInstance.BusinessId);
    }

    public async Task<WorkflowProcessInstance?> GetActiveByBusinessIdAsync(
    string businessId,
    CancellationToken cancellationToken = default)
    {
        var request = new
        {
            filter = new
            {
                businessId,
                state = "ACTIVE"
            },
            page = new
            {
                limit = 10
            }
        };

        using var response =
            await httpClient.PostAsJsonAsync(
                "/v2/process-instances/search",
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<CamundaProcessInstanceSearchResponse>(
                    cancellationToken)
            ?? throw new InvalidOperationException(
                "Camunda returned an empty Process Instance search response.");

        if (result.Items.Count == 0)
        {
            return null;
        }

        if (result.Items.Count > 1)
        {
            throw new InvalidOperationException(
                $"More than one active process instance was found " +
                $"for business id '{businessId}'.");
        }

        var item =
            result.Items[0];

        return new WorkflowProcessInstance(
            item.ProcessInstanceKey,
            item.ProcessDefinitionId,
            item.State,
            item.BusinessId);
    }
}