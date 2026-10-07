using FlowOps.BuildingBlocks.Domain.Results;
using FlowOps.BuildingBlocks.Domain.Time;
using FlowOps.Modules.Procurement.Application.Abstractions.Persistence;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests;

namespace FlowOps.Modules.Procurement.Application.PurchaseRequests.SubmitPurchaseRequest;

public sealed class SubmitPurchaseRequestHandler
{
    private readonly IPurchaseRequestRepository _repository;
    private readonly IProcurementUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public SubmitPurchaseRequestHandler(
        IPurchaseRequestRepository repository,
        IProcurementUnitOfWork unitOfWork,
        IClock clock)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<SubmitPurchaseRequestResponse>> HandleAsync(
        SubmitPurchaseRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.PurchaseRequestId == Guid.Empty)
        {
            return Result<SubmitPurchaseRequestResponse>.Failure(
                SubmitPurchaseRequestErrors.NotFound);
        }

        var purchaseRequest =
            await _repository.GetByIdAsync(
                new PurchaseRequestId(
                    command.PurchaseRequestId),
                cancellationToken);

        if (purchaseRequest is null)
        {
            return Result<SubmitPurchaseRequestResponse>.Failure(
                SubmitPurchaseRequestErrors.NotFound);
        }

        var submitResult =
            purchaseRequest.Submit(
                _clock.UtcNow);

        if (submitResult.IsFailure)
        {
            return Result<SubmitPurchaseRequestResponse>.Failure(
                submitResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<SubmitPurchaseRequestResponse>.Success(
            new SubmitPurchaseRequestResponse(
                purchaseRequest.Id.Value,
                purchaseRequest.RequestNumber.Value,
                purchaseRequest.Status.ToString(),
                purchaseRequest.SubmittedAtUtc!.Value));
    }
}