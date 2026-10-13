namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

internal sealed class CamundaJobWorkerOptions
{
    public const string SectionName = "CamundaJobWorker";

    public string WorkerName { get; init; } =
        "flowops-procurement-worker";

    public int MaxJobsToActivate { get; init; } = 10;

    public long JobTimeoutMilliseconds { get; init; } = 30_000;

    public long RequestTimeoutMilliseconds { get; init; } = 5_000;

    public int EmptyPollDelayMilliseconds { get; init; } = 1_000;
}