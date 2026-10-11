using FlowOps.BuildingBlocks.Domain.Events;
using FlowOps.BuildingBlocks.Domain.Time;
using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests;
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
                await ProcessBatchAsync(
                    stoppingToken);
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

        var workflowTaskReader =
            scope.ServiceProvider
                .GetRequiredService<
                    IWorkflowTaskReader>();

        var workflowProcessReader =
            scope.ServiceProvider
                .GetRequiredService<
                    IWorkflowProcessReader>();

        var workflowTaskCompleter =
            scope.ServiceProvider
                .GetRequiredService<
                    IWorkflowTaskCompleter>();

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
                .ToListAsync(
                    cancellationToken);

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
                    dbContext,
                    workflowStarter,
                    workflowTaskReader,
                    workflowProcessReader,
                    workflowTaskCompleter,
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
        ProcurementDbContext dbContext,
        IPurchaseRequestWorkflowStarter workflowStarter,
        IWorkflowTaskReader workflowTaskReader,
        IWorkflowProcessReader workflowProcessReader,
        IWorkflowTaskCompleter workflowTaskCompleter,
        CancellationToken cancellationToken)
    {
        var approvalStepApprovedEventType =
            typeof(PurchaseRequestApprovalStepApprovedDomainEvent)
                .FullName;

        var submittedEventType =
            typeof(PurchaseRequestSubmittedDomainEvent)
                .FullName;

        var approvedEventType =
            typeof(PurchaseRequestApprovedDomainEvent)
                .FullName;

        var rejectedEventType =
            typeof(PurchaseRequestRejectedDomainEvent)
                .FullName;

        //
        // PurchaseRequestSubmittedDomainEvent
        //
        if (string.Equals(
                message.Type,
                submittedEventType,
                StringComparison.Ordinal))
        {
            await HandleSubmittedAsync(
                message,
                dbContext,
                workflowStarter,
                submittedEventType!,
                cancellationToken);

            return;
        }

        //
        // PurchaseRequestApprovalStepApprovedDomainEvent
        //
        if (string.Equals(
                message.Type,
                approvalStepApprovedEventType,
                StringComparison.Ordinal))
        {
            await HandleApprovalStepApprovedAsync(
                message,
                workflowTaskReader,
                workflowProcessReader,
                workflowTaskCompleter,
                approvalStepApprovedEventType!,
                cancellationToken);

            return;
        }

        //
        // PurchaseRequestApprovedDomainEvent
        //
        if (string.Equals(
                message.Type,
                approvedEventType,
                StringComparison.Ordinal))
        {
            _logger.LogInformation(
                "Final Purchase Request business approval recorded. " +
                "OutboxMessageId={MessageId}.",
                message.Id);

            return;
        }

        //
        // PurchaseRequestRejectedDomainEvent
        //
        if (string.Equals(
                message.Type,
                rejectedEventType,
                StringComparison.Ordinal))
        {
            await HandleRejectedAsync(
                message,
                workflowTaskReader,
                workflowProcessReader,
                workflowTaskCompleter,
                rejectedEventType!,
                cancellationToken);

            return;
        }

        //
        // Events that currently require no external integration
        //
        _logger.LogInformation(
            "No external integration action is required for Outbox message {MessageId}, Type {MessageType}.",
            message.Id,
            message.Type);
    }

    private async Task HandleSubmittedAsync(
        OutboxMessage message,
        ProcurementDbContext dbContext,
        IPurchaseRequestWorkflowStarter workflowStarter,
        string submittedEventType,
        CancellationToken cancellationToken)
    {
        var domainEvent =
            JsonSerializer.Deserialize<
                PurchaseRequestSubmittedDomainEvent>(
                message.Content,
                JsonOptions);

        if (domainEvent is null)
        {
            throw new InvalidOperationException(
                $"Could not deserialize Outbox message " +
                $"'{message.Id}' as '{submittedEventType}'.");
        }

        var purchaseRequest =
            await dbContext.PurchaseRequests
                .Include(x => x.Items)
                .SingleOrDefaultAsync(
                    x =>
                        x.Id ==
                        domainEvent.PurchaseRequestId,
                    cancellationToken);

        if (purchaseRequest is null)
        {
            throw new InvalidOperationException(
                $"Purchase request " +
                $"'{domainEvent.PurchaseRequestId.Value}' " +
                "was not found before workflow start.");
        }

        var workflowRequest =
            new StartPurchaseRequestWorkflowRequest(
                domainEvent.PurchaseRequestId.Value,
                domainEvent.RequesterId.Value,
                domainEvent.DepartmentId.Value,
                purchaseRequest.TotalAmount);

        var result =
            await workflowStarter.StartAsync(
                workflowRequest,
                cancellationToken);

        _logger.LogInformation(
            "Camunda Purchase Request workflow started. " +
            "OutboxMessageId={MessageId}, " +
            "PurchaseRequestId={PurchaseRequestId}, " +
            "TotalAmount={TotalAmount}, " +
            "ProcessInstanceKey={ProcessInstanceKey}, " +
            "ProcessDefinitionVersion={ProcessDefinitionVersion}.",
            message.Id,
            domainEvent.PurchaseRequestId.Value,
            purchaseRequest.TotalAmount,
            result.ProcessInstanceKey,
            result.ProcessDefinitionVersion);

        if (purchaseRequest is null)
        {
            throw new InvalidOperationException(
                $"Purchase request " +
                $"'{domainEvent.PurchaseRequestId.Value}' " +
                "was not found after workflow start.");
        }

        var markInApprovalResult =
            purchaseRequest.MarkInApproval();

        if (markInApprovalResult.IsFailure)
        {
            throw new InvalidOperationException(
                $"Purchase request " +
                $"'{domainEvent.PurchaseRequestId.Value}' " +
                "could not transition to InApproval. " +
                $"Error: {markInApprovalResult.Error.Code}.");
        }

        _logger.LogInformation(
            "Purchase Request {PurchaseRequestId} transitioned " +
            "from Submitted to InApproval. " +
            "ProcessInstanceKey={ProcessInstanceKey}.",
            domainEvent.PurchaseRequestId.Value,
            result.ProcessInstanceKey);
    }

    private async Task HandleApprovalStepApprovedAsync(
        OutboxMessage message,
        IWorkflowTaskReader workflowTaskReader,
        IWorkflowProcessReader workflowProcessReader,
        IWorkflowTaskCompleter workflowTaskCompleter,
        string approvalStepApprovedEventType,
        CancellationToken cancellationToken)
    {
        var domainEvent =
            JsonSerializer.Deserialize<
                PurchaseRequestApprovalStepApprovedDomainEvent>(
                message.Content,
                JsonOptions);

        if (domainEvent is null)
        {
            throw new InvalidOperationException(
                $"Could not deserialize Outbox message " +
                $"'{message.Id}' as " +
                $"'{approvalStepApprovedEventType}'.");
        }

        var businessId =
            domainEvent.PurchaseRequestId.Value
                .ToString();

        var processInstance =
            await workflowProcessReader
                .GetActiveByBusinessIdAsync(
                    businessId,
                    cancellationToken);

        if (processInstance is null)
        {
            throw new InvalidOperationException(
                $"No active workflow process was found for " +
                $"PurchaseRequest '{businessId}'.");
        }

        var task =
            await workflowTaskReader
                .GetActiveTaskForProcessAsync(
                    processInstance.ProcessInstanceKey,
                    cancellationToken);

        if (task is null)
        {
            throw new InvalidOperationException(
                $"No active workflow task was found for " +
                $"process instance " +
                $"'{processInstance.ProcessInstanceKey}'.");
        }

        var expectedElementId =
            domainEvent.Stage switch
            {
                PurchaseRequestApprovalStage.Manager =>
                    "UserTask_ManagerApproval",

                PurchaseRequestApprovalStage.Finance =>
                    "UserTask_FinanceApproval",

                PurchaseRequestApprovalStage.Director =>
                    "UserTask_DirectorApproval",

                PurchaseRequestApprovalStage.Procurement =>
                    "UserTask_ProcurementApproval",

                _ =>
                    throw new InvalidOperationException(
                        $"Unsupported approval stage " +
                        $"'{domainEvent.Stage}'.")
            };

        if (!string.Equals(
                task.ElementId,
                expectedElementId,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Workflow approval stage mismatch. " +
                $"Expected ElementId='{expectedElementId}', " +
                $"Actual ElementId='{task.ElementId}', " +
                $"Stage='{domainEvent.Stage}', " +
                $"TaskKey='{task.TaskKey}'.");
        }

        var completed =
            await workflowTaskCompleter.CompleteAsync(
                task.TaskKey,
                new CompleteWorkflowTaskRequest(
                    "approve",
                    new Dictionary<string, object?>
                    {
                        ["decision"] = "approved"
                    }),
                cancellationToken);

        if (!completed)
        {
            throw new InvalidOperationException(
                $"Workflow task '{task.TaskKey}' " +
                "could not be completed.");
        }

        _logger.LogInformation(
            "Camunda Purchase Request approval step completed. " +
            "OutboxMessageId={MessageId}, " +
            "PurchaseRequestId={PurchaseRequestId}, " +
            "Stage={Stage}, " +
            "IsFinalApproval={IsFinalApproval}, " +
            "ProcessInstanceKey={ProcessInstanceKey}, " +
            "TaskKey={TaskKey}, " +
            "ElementId={ElementId}.",
            message.Id,
            businessId,
            domainEvent.Stage,
            domainEvent.IsFinalApproval,
            processInstance.ProcessInstanceKey,
            task.TaskKey,
            task.ElementId);
    }

    private async Task HandleRejectedAsync(
        OutboxMessage message,
        IWorkflowTaskReader workflowTaskReader,
        IWorkflowProcessReader workflowProcessReader,
        IWorkflowTaskCompleter workflowTaskCompleter,
        string rejectedEventType,
        CancellationToken cancellationToken)
    {
        var domainEvent =
            JsonSerializer.Deserialize<
                PurchaseRequestRejectedDomainEvent>(
                message.Content,
                JsonOptions);

        if (domainEvent is null)
        {
            throw new InvalidOperationException(
                $"Could not deserialize Outbox message " +
                $"'{message.Id}' as '{rejectedEventType}'.");
        }

        var businessId =
            domainEvent.PurchaseRequestId.Value
                .ToString();

        var processInstance =
            await workflowProcessReader
                .GetActiveByBusinessIdAsync(
                    businessId,
                    cancellationToken);

        if (processInstance is null)
        {
            throw new InvalidOperationException(
                $"No active workflow process was found for " +
                $"PurchaseRequest '{businessId}'.");
        }

        var task =
            await workflowTaskReader
                .GetActiveTaskForProcessAsync(
                    processInstance.ProcessInstanceKey,
                    cancellationToken);

        if (task is null)
        {
            throw new InvalidOperationException(
                $"No active workflow task was found for " +
                $"process instance " +
                $"'{processInstance.ProcessInstanceKey}'.");
        }

        var completed =
            await workflowTaskCompleter.CompleteAsync(
                task.TaskKey,
                new CompleteWorkflowTaskRequest(
                    "reject",
                    new Dictionary<string, object?>
                    {
                        ["decision"] = "rejected"
                    }),
                cancellationToken);

        if (!completed)
        {
            throw new InvalidOperationException(
                $"Workflow task '{task.TaskKey}' " +
                "could not be completed.");
        }

        _logger.LogInformation(
            "Camunda Purchase Request rejection task completed. " +
            "OutboxMessageId={MessageId}, " +
            "PurchaseRequestId={PurchaseRequestId}, " +
            "ProcessInstanceKey={ProcessInstanceKey}, " +
            "TaskKey={TaskKey}.",
            message.Id,
            businessId,
            processInstance.ProcessInstanceKey,
            task.TaskKey);
    }
}