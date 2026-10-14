using FlowOps.BuildingBlocks.Domain.Results;
using FlowOps.Modules.Procurement.Application.Abstractions.Persistence;
using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests;

namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.AcknowledgePurchaseRequest;

public sealed class AcknowledgePurchaseRequestHandler
{
    private const string RequesterAcknowledgedMessageName =
        "procurement.requester-acknowledged";

    private readonly IPurchaseRequestRepository _repository;
    private readonly IWorkflowMessageCorrelator _workflowMessageCorrelator;

    public AcknowledgePurchaseRequestHandler(
        IPurchaseRequestRepository repository,
        IWorkflowMessageCorrelator workflowMessageCorrelator)
    {
        _repository = repository;
        _workflowMessageCorrelator = workflowMessageCorrelator;
    }

    public async Task<Result<AcknowledgePurchaseRequestResponse>> HandleAsync(
        AcknowledgePurchaseRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.PurchaseRequestId == Guid.Empty)
        {
            return Result<AcknowledgePurchaseRequestResponse>.Failure(
                Errors.NotFound);
        }

        var purchaseRequest =
            await _repository.GetByIdAsync(
                new PurchaseRequestId(
                    command.PurchaseRequestId),
                cancellationToken);

        if (purchaseRequest is null)
        {
            return Result<AcknowledgePurchaseRequestResponse>.Failure(
                Errors.NotFound);
        }

        var correlationResult =
            await _workflowMessageCorrelator.CorrelateAsync(
                RequesterAcknowledgedMessageName,
                command.PurchaseRequestId.ToString(),
                cancellationToken);

        if (correlationResult is null)
        {
            return Result<AcknowledgePurchaseRequestResponse>.Failure(
                Errors.WorkflowMessageNotCorrelated);
        }

        return Result<AcknowledgePurchaseRequestResponse>.Success(
            new AcknowledgePurchaseRequestResponse(
                command.PurchaseRequestId,
                correlationResult.MessageKey,
                correlationResult.ProcessInstanceKey));
    }
}