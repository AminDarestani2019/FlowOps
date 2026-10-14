using System.Net.Http.Json;

namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

internal sealed class CamundaJobClient(
    HttpClient httpClient)
{
    public async Task<IReadOnlyList<CamundaActivatedJobDto>> ActivateAsync(
        string jobType,
        string workerName,
        int maxJobsToActivate = 10,
        long timeoutMilliseconds = 30_000,
        long requestTimeoutMilliseconds = 10_000,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            type = jobType,
            worker = workerName,
            timeout = timeoutMilliseconds,
            maxJobsToActivate,
            requestTimeout = requestTimeoutMilliseconds,
            fetchVariable = Array.Empty<string>()
        };

        using var response =
            await httpClient.PostAsJsonAsync(
                "/v2/jobs/activation",
                payload,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<CamundaJobActivationResponse>(
                    cancellationToken: cancellationToken);

        return result?.Jobs ?? [];
    }

    public async Task CompleteAsync(
        string jobKey,
        IReadOnlyDictionary<string, object?>? variables = null,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            variables =
                variables ??
                new Dictionary<string, object?>()
        };

        using var response =
            await httpClient.PostAsJsonAsync(
                $"/v2/jobs/{jobKey}/completion",
                payload,
                cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task ThrowErrorAsync(
        string jobKey,
        string errorCode,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);

        var payload = new
        {
            errorCode,
            errorMessage
        };

        using var response =
            await httpClient.PostAsJsonAsync(
                $"/v2/jobs/{jobKey}/error",
                payload,
                cancellationToken);

        response.EnsureSuccessStatusCode();
    }

}