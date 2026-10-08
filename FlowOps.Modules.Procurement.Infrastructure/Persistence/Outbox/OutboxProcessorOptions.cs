namespace FlowOps.Modules.Procurement.Infrastructure.Persistence.Outbox;

internal sealed class OutboxProcessorOptions
{
    public const string SectionName = "Procurement:Outbox";

    public int PollingIntervalSeconds { get; init; } = 5;

    public int BatchSize { get; init; } = 20;

    public int MaxAttempts { get; init; } = 5;
}
