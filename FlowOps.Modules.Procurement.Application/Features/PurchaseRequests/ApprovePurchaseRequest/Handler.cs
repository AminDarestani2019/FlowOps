using FlowOps.BuildingBlocks.Domain.Results;
using FlowOps.BuildingBlocks.Domain.Time;
using FlowOps.Modules.Procurement.Application.Abstractions.Persistence;
using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests;
using System.Text.Json;

namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.ApprovePurchaseRequest;

public sealed class ApprovePurchaseRequestHandler
{
    private const string ManagerTaskElementId =
        "UserTask_ManagerApproval";

    private const string FinanceTaskElementId =
        "UserTask_FinanceApproval";

    private const string DirectorTaskElementId =
        "UserTask_DirectorApproval";

    private const string ProcurementTaskElementId =
        "UserTask_ProcurementApproval";

    private readonly IPurchaseRequestRepository _repository;
    private readonly IProcurementUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IWorkflowTaskReader _workflowTaskReader;
    private readonly IWorkflowProcessReader _workflowProcessReader;
    private readonly IWorkflowVariableReader _workflowVariableReader;

    public ApprovePurchaseRequestHandler(
        IPurchaseRequestRepository repository,
        IProcurementUnitOfWork unitOfWork,
        IClock clock,
        IWorkflowTaskReader workflowTaskReader,
        IWorkflowProcessReader workflowProcessReader,
        IWorkflowVariableReader workflowVariableReader)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _workflowTaskReader = workflowTaskReader;
        _workflowProcessReader = workflowProcessReader;
        _workflowVariableReader = workflowVariableReader;
    }

    public async Task<Result<ApprovePurchaseRequestResponse>> HandleAsync(
        ApprovePurchaseRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.PurchaseRequestId == Guid.Empty)
        {
            return Result<ApprovePurchaseRequestResponse>.Failure(
                Errors.NotFound);
        }

        if (string.IsNullOrWhiteSpace(
                command.TaskKey))
        {
            return Result<ApprovePurchaseRequestResponse>.Failure(
                Errors.WorkflowTaskNotFound);
        }

        var task =
            await _workflowTaskReader.GetTaskAsync(
                command.TaskKey,
                cancellationToken);

        if (task is null)
        {
            return Result<ApprovePurchaseRequestResponse>.Failure(
                Errors.WorkflowTaskNotFound);
        }

        if (!string.Equals(
                task.State,
                "CREATED",
                StringComparison.OrdinalIgnoreCase))
        {
            return Result<ApprovePurchaseRequestResponse>.Failure(
                Errors.WorkflowTaskNotActive);
        }

        var processInstance =
            await _workflowProcessReader.GetByKeyAsync(
                task.ProcessInstanceKey,
                cancellationToken);

        if (processInstance is null)
        {
            return Result<ApprovePurchaseRequestResponse>.Failure(
                Errors.WorkflowProcessNotFound);
        }

        var expectedBusinessId =
            command.PurchaseRequestId.ToString();

        if (!string.Equals(
                processInstance.BusinessId,
                expectedBusinessId,
                StringComparison.OrdinalIgnoreCase))
        {
            return Result<ApprovePurchaseRequestResponse>.Failure(
                Errors.WorkflowCorrelationMismatch);
        }

        var approvalLevelVariable =
            await _workflowVariableReader.GetAsync(
                task.ProcessInstanceKey,
                "approvalLevel",
                cancellationToken);

        if (approvalLevelVariable is null)
        {
            return Result<ApprovePurchaseRequestResponse>.Failure(
                Errors.WorkflowApprovalLevelNotFound);
        }

        string? approvalLevel;

        try
        {
            approvalLevel =
                JsonSerializer.Deserialize<string>(
                    approvalLevelVariable.Value);
        }
        catch (JsonException)
        {
            return Result<ApprovePurchaseRequestResponse>.Failure(
                Errors.WorkflowApprovalContextInvalid);
        }

        if (string.IsNullOrWhiteSpace(
                approvalLevel))
        {
            return Result<ApprovePurchaseRequestResponse>.Failure(
                Errors.WorkflowApprovalContextInvalid);
        }

        if (!TryResolveApprovalStep(
                task.ElementId,
                approvalLevel,
                out var stage,
                out var isFinalApproval))
        {
            return Result<ApprovePurchaseRequestResponse>.Failure(
                Errors.WorkflowApprovalContextInvalid);
        }

        var purchaseRequest =
            await _repository.GetByIdAsync(
                new PurchaseRequestId(
                    command.PurchaseRequestId),
                cancellationToken);

        if (purchaseRequest is null)
        {
            return Result<ApprovePurchaseRequestResponse>.Failure(
                Errors.NotFound);
        }

        var approveResult =
            purchaseRequest.ApproveStep(
                stage,
                isFinalApproval,
                _clock.UtcNow);

        if (approveResult.IsFailure)
        {
            return Result<ApprovePurchaseRequestResponse>.Failure(
                approveResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<ApprovePurchaseRequestResponse>.Success(
            new ApprovePurchaseRequestResponse(
                purchaseRequest.Id.Value,
                purchaseRequest.Status.ToString()));
    }

    private static bool TryResolveApprovalStep(
        string elementId,
        string approvalLevel,
        out PurchaseRequestApprovalStage stage,
        out bool isFinalApproval)
    {
        stage = default;
        isFinalApproval = false;

        var normalizedLevel =
            approvalLevel.Trim().ToUpperInvariant();

        var validApprovalLevel =
            normalizedLevel is
                "MANAGER" or
                "MANAGER_FINANCE" or
                "MANAGER_FINANCE_DIRECTOR";

        if (!validApprovalLevel)
        {
            return false;
        }

        if (string.Equals(
                elementId,
                ManagerTaskElementId,
                StringComparison.Ordinal))
        {
            stage =
                PurchaseRequestApprovalStage.Manager;

            isFinalApproval = false;

            return true;
        }

        if (string.Equals(
                elementId,
                FinanceTaskElementId,
                StringComparison.Ordinal))
        {
            if (normalizedLevel is not
                ("MANAGER_FINANCE" or
                "MANAGER_FINANCE_DIRECTOR"))
            {
                return false;
            }

            stage =
                PurchaseRequestApprovalStage.Finance;

            isFinalApproval = false;

            return true;
        }

        if (string.Equals(
                elementId,
                DirectorTaskElementId,
                StringComparison.Ordinal))
        {
            if (normalizedLevel !=
                "MANAGER_FINANCE_DIRECTOR")
            {
                return false;
            }

            stage =
                PurchaseRequestApprovalStage.Director;

            isFinalApproval = false;

            return true;
        }

        if (string.Equals(
                elementId,
                ProcurementTaskElementId,
                StringComparison.Ordinal))
        {
            stage =
                PurchaseRequestApprovalStage.Procurement;

            isFinalApproval = true;

            return true;
        }

        return false;
    }
}