using FlowOps.BuildingBlocks.Domain.Time;
using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FlowOps.Modules.Procurement.Infrastructure.Persistence.Outbox;

internal sealed class OutboxProcessor : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly OutboxProcessorOptions _options;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxProcessorOptions> options,
        ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Procurement Outbox Processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unexpected error while processing Procurement Outbox.");
            }

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(
                        _options.PollingIntervalSeconds),
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation(
            "Procurement Outbox Processor stopped.");
    }

    private async Task ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        using var scope =
            _scopeFactory.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<ProcurementDbContext>();

        var workflowStarter =
            scope.ServiceProvider
                .GetRequiredService<
                    IPurchaseRequestWorkflowStarter>();

        var clock =
            scope.ServiceProvider
                .GetRequiredService<IClock>();

        var messages =
            await dbContext.OutboxMessages
                .Where(x =>
                    x.ProcessedOnUtc == null &&
                    x.AttemptCount < _options.MaxAttempts)
                .OrderBy(x => x.OccurredOnUtc)
                .Take(_options.BatchSize)
                .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        _logger.LogInformation(
            "Found {Count} pending Procurement Outbox message(s).",
            messages.Count);

        foreach (var message in messages)
        {
            try
            {
                await DispatchAsync(
                    message,
                    workflowStarter,
                    cancellationToken);

                message.MarkProcessed(
                    clock.UtcNow);

                await dbContext.SaveChangesAsync(
                    cancellationToken);

                _logger.LogInformation(
                    "Outbox message {MessageId} processed successfully.",
                    message.Id);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                message.MarkFailed(
                    exception.ToString());

                await dbContext.SaveChangesAsync(
                    cancellationToken);

                _logger.LogError(
                    exception,
                    "Failed to process Outbox message {MessageId}. Attempt {AttemptCount}/{MaxAttempts}.",
                    message.Id,
                    message.AttemptCount,
                    _options.MaxAttempts);
            }
        }
    }

    private async Task DispatchAsync(
        OutboxMessage message,
        IPurchaseRequestWorkflowStarter workflowStarter,
        CancellationToken cancellationToken)
    {
        var submittedEventType =
            typeof(PurchaseRequestSubmittedDomainEvent)
                .FullName;

        if (!string.Equals(
                message.Type,
                submittedEventType,
                StringComparison.Ordinal))
        {
            _logger.LogInformation(
                "No external integration action is required for Outbox message {MessageId}, Type {MessageType}.",
                message.Id,
                message.Type);

            return;
        }

        var domainEvent =
            JsonSerializer.Deserialize<
                PurchaseRequestSubmittedDomainEvent>(
                message.Content,
                JsonOptions);

        if (domainEvent is null)
        {
            throw new InvalidOperationException(
                $"Could not deserialize Outbox message '{message.Id}' as '{submittedEventType}'.");
        }

        var workflowRequest =
            new StartPurchaseRequestWorkflowRequest(
                domainEvent.PurchaseRequestId.Value,
                domainEvent.RequesterId.Value,
                domainEvent.DepartmentId.Value);

        var result =
            await workflowStarter.StartAsync(
                workflowRequest,
                cancellationToken);

        _logger.LogInformation(
            "Camunda Purchase Request workflow started. OutboxMessageId={MessageId}, PurchaseRequestId={PurchaseRequestId}, ProcessInstanceKey={ProcessInstanceKey}, ProcessDefinitionVersion={ProcessDefinitionVersion}.",
            message.Id,
            domainEvent.PurchaseRequestId.Value,
            result.ProcessInstanceKey,
            result.ProcessDefinitionVersion);
    }
}