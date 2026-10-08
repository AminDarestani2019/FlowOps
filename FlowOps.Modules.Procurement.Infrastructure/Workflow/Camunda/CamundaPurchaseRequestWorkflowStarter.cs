using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

public sealed class CamundaPurchaseRequestWorkflowStarter
    : IPurchaseRequestWorkflowStarter
{
    private readonly HttpClient _httpClient;
    private readonly CamundaOptions _options;

    public CamundaPurchaseRequestWorkflowStarter(
        HttpClient httpClient,
        IOptions<CamundaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<StartPurchaseRequestWorkflowResult> StartAsync(
        StartPurchaseRequestWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            processDefinitionId =
                _options.PurchaseRequestApprovalProcessId,

            businessId =
                request.PurchaseRequestId.ToString(),

            variables = new
            {
                purchaseRequestId =
                    request.PurchaseRequestId.ToString(),

                requesterId =
                    request.RequesterId.ToString(),

                departmentId =
                    request.DepartmentId.ToString()
            }
        };

        using var response =
            await _httpClient.PostAsJsonAsync(
                "/v2/process-instances",
                payload,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    CamundaCreateProcessInstanceResponse>(
                    cancellationToken:
                        cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException(
                "Camunda returned an empty process instance response.");
        }

        return new StartPurchaseRequestWorkflowResult(
            result.ProcessInstanceKey,
            result.ProcessDefinitionKey,
            result.ProcessDefinitionVersion,
            result.BusinessId
                ?? request.PurchaseRequestId.ToString());
    }
}
