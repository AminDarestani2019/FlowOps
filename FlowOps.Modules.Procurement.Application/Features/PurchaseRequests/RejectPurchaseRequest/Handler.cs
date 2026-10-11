using FlowOps.BuildingBlocks.Domain.Results;
using FlowOps.BuildingBlocks.Domain.Time;
using FlowOps.Modules.Procurement.Application.Abstractions.Persistence;
using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests;

namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.RejectPurchaseRequest;

public sealed class RejectPurchaseRequestHandler
{
    private readonly IPurchaseRequestRepository _repository;
    private readonly IProcurementUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IWorkflowTaskReader _workflowTaskReader;
    private readonly IWorkflowProcessReader _workflowProcessReader;

    public RejectPurchaseRequestHandler(
        IPurchaseRequestRepository repository,
        IProcurementUnitOfWork unitOfWork,
        IClock clock,
        IWorkflowTaskReader workflowTaskReader,
        IWorkflowProcessReader workflowProcessReader)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _workflowTaskReader = workflowTaskReader;
        _workflowProcessReader = workflowProcessReader;
    }

    public async Task<Result<RejectPurchaseRequestResponse>> HandleAsync(
        RejectPurchaseRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.PurchaseRequestId == Guid.Empty)
        {
            return Result<RejectPurchaseRequestResponse>.Failure(
                Errors.NotFound);
        }

        if (string.IsNullOrWhiteSpace(
                command.TaskKey))
        {
            return Result<RejectPurchaseRequestResponse>.Failure(
                Errors.WorkflowTaskNotFound);
        }

        var task =
            await _workflowTaskReader.GetTaskAsync(
                command.TaskKey,
                cancellationToken);

        if (task is null)
        {
            return Result<RejectPurchaseRequestResponse>.Failure(
                Errors.WorkflowTaskNotFound);
        }

        if (!string.Equals(
                task.State,
                "CREATED",
                StringComparison.OrdinalIgnoreCase))
        {
            return Result<RejectPurchaseRequestResponse>.Failure(
                Errors.WorkflowTaskNotActive);
        }

        var processInstance =
            await _workflowProcessReader.GetByKeyAsync(
                task.ProcessInstanceKey,
                cancellationToken);

        if (processInstance is null)
        {
            return Result<RejectPurchaseRequestResponse>.Failure(
                Errors.WorkflowProcessNotFound);
        }

        var expectedBusinessId =
            command.PurchaseRequestId.ToString();

        if (!string.Equals(
                processInstance.BusinessId,
                expectedBusinessId,
                StringComparison.OrdinalIgnoreCase))
        {
            return Result<RejectPurchaseRequestResponse>.Failure(
                Errors.WorkflowCorrelationMismatch);
        }

        var purchaseRequest =
            await _repository.GetByIdAsync(
                new PurchaseRequestId(
                    command.PurchaseRequestId),
                cancellationToken);

        if (purchaseRequest is null)
        {
            return Result<RejectPurchaseRequestResponse>.Failure(
                Errors.NotFound);
        }

        var rejectResult =
            purchaseRequest.Reject(
                _clock.UtcNow);

        if (rejectResult.IsFailure)
        {
            return Result<RejectPurchaseRequestResponse>.Failure(
                rejectResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<RejectPurchaseRequestResponse>.Success(
            new RejectPurchaseRequestResponse(
                purchaseRequest.Id.Value,
                purchaseRequest.Status.ToString()));
    }
}