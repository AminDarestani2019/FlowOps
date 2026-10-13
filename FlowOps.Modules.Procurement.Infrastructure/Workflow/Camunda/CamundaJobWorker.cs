using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

internal sealed class CamundaJobWorker(
    CamundaJobClient jobClient,
    IOptions<CamundaJobWorkerOptions> options,
    ILogger<CamundaJobWorker> logger)
    : BackgroundService
{
    private const string RecordApprovalAuditJobType =
        "flowops.procurement.record-approval-audit";

    private const string NotifyRequesterJobType =
        "flowops.procurement.notify-requester";

    private const string StandardFollowUpJobType =
        "flowops.procurement.standard-follow-up";

    private const string FinanceFollowUpJobType =
        "flowops.procurement.finance-follow-up";

    private const string ExecutiveFollowUpJobType =
        "flowops.procurement.executive-follow-up";


    private readonly CamundaJobWorkerOptions _options =
        options.Value;

    protected override Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Camunda Job Worker starting. WorkerName={WorkerName}",
            _options.WorkerName);

        return Task.WhenAll(
            RunWorkerLoopAsync(
                RecordApprovalAuditJobType,
                HandleRecordApprovalAuditAsync,
                stoppingToken),

            RunWorkerLoopAsync(
                NotifyRequesterJobType,
                HandleNotifyRequesterAsync,
                stoppingToken),

            RunWorkerLoopAsync(
                StandardFollowUpJobType,
                HandleStandardFollowUpAsync,
                stoppingToken),

            RunWorkerLoopAsync(
                FinanceFollowUpJobType,
                HandleFinanceFollowUpAsync,
                stoppingToken),

            RunWorkerLoopAsync(
                ExecutiveFollowUpJobType,
                HandleExecutiveFollowUpAsync,
                stoppingToken));
    }

    private async Task RunWorkerLoopAsync(
        string jobType,
        Func<CamundaActivatedJobDto, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var jobs =
                    await jobClient.ActivateAsync(
                        jobType,
                        _options.WorkerName,
                        _options.MaxJobsToActivate,
                        _options.JobTimeoutMilliseconds,
                        _options.RequestTimeoutMilliseconds,
                        cancellationToken);

                if (jobs.Count == 0)
                {
                    await Task.Delay(
                        _options.EmptyPollDelayMilliseconds,
                        cancellationToken);

                    continue;
                }

                foreach (var job in jobs)
                {
                    await ProcessJobAsync(
                        job,
                        handler,
                        cancellationToken);
                }
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Camunda job polling failed. JobType={JobType}",
                    jobType);

                await Task.Delay(
                    _options.EmptyPollDelayMilliseconds,
                    cancellationToken);
            }
        }
    }

    private async Task ProcessJobAsync(
        CamundaActivatedJobDto job,
        Func<CamundaActivatedJobDto, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Processing Camunda job. JobKey={JobKey}, JobType={JobType}, ProcessInstanceKey={ProcessInstanceKey}, ElementId={ElementId}",
                job.JobKey,
                job.Type,
                job.ProcessInstanceKey,
                job.ElementId);

            await handler(
                job,
                cancellationToken);

            await jobClient.CompleteAsync(
                job.JobKey,
                cancellationToken: cancellationToken);

            logger.LogInformation(
                "Camunda job completed. JobKey={JobKey}, JobType={JobType}",
                job.JobKey,
                job.Type);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Camunda job processing failed. JobKey={JobKey}, JobType={JobType}",
                job.JobKey,
                job.Type);

            // Intentionally do not complete the job.
            // Camunda job timeout/retry semantics will handle redelivery.
            throw;
        }
    }

    private Task HandleRecordApprovalAuditAsync(
        CamundaActivatedJobDto job,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Approval audit recorded. ProcessInstanceKey={ProcessInstanceKey}, ElementId={ElementId}",
            job.ProcessInstanceKey,
            job.ElementId);

        return Task.CompletedTask;
    }

    private Task HandleNotifyRequesterAsync(
        CamundaActivatedJobDto job,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Requester notification simulated. ProcessInstanceKey={ProcessInstanceKey}, ElementId={ElementId}",
            job.ProcessInstanceKey,
            job.ElementId);

        return Task.CompletedTask;
    }

    private Task HandleStandardFollowUpAsync(
        CamundaActivatedJobDto job,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Standard follow-up executed. ProcessInstanceKey={ProcessInstanceKey}, ElementId={ElementId}",
            job.ProcessInstanceKey,
            job.ElementId);

        return Task.CompletedTask;
    }

    private Task HandleFinanceFollowUpAsync(
        CamundaActivatedJobDto job,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Finance follow-up executed. ProcessInstanceKey={ProcessInstanceKey}, ElementId={ElementId}",
            job.ProcessInstanceKey,
            job.ElementId);

        return Task.CompletedTask;
    }

    private Task HandleExecutiveFollowUpAsync(
        CamundaActivatedJobDto job,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executive follow-up executed. ProcessInstanceKey={ProcessInstanceKey}, ElementId={ElementId}",
            job.ProcessInstanceKey,
            job.ElementId);

        return Task.CompletedTask;
    }
}