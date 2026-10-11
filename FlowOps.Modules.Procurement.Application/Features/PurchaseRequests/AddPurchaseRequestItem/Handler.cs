using FlowOps.BuildingBlocks.Domain.Results;
using FlowOps.Modules.Procurement.Application.Abstractions.Persistence;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;

namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.AddPurchaseRequestItem;

public sealed class AddPurchaseRequestItemHandler
{
    private readonly IPurchaseRequestRepository _repository;
    private readonly IProcurementUnitOfWork _unitOfWork;

    public AddPurchaseRequestItemHandler(
        IPurchaseRequestRepository repository,
        IProcurementUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AddPurchaseRequestItemResponse>> HandleAsync(
        AddPurchaseRequestItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult =
            AddPurchaseRequestItemValidator.Validate(command);

        if (validationResult.IsFailure)
        {
            return Result<AddPurchaseRequestItemResponse>.Failure(
                validationResult.Error);
        }

        var purchaseRequestId =
            new PurchaseRequestId(command.PurchaseRequestId);

        var purchaseRequest =
            await _repository.GetByIdAsync(
                purchaseRequestId,
                cancellationToken);

        if (purchaseRequest is null)
        {
            return Result<AddPurchaseRequestItemResponse>.Failure(
                AddPurchaseRequestItemErrors.PurchaseRequestNotFound);
        }

        var moneyResult =
            Money.Create(
                command.UnitPrice,
                purchaseRequest.Currency);

        if (moneyResult.IsFailure)
        {
            return Result<AddPurchaseRequestItemResponse>.Failure(
                moneyResult.Error);
        }

        var addItemResult =
            purchaseRequest.AddItem(
                command.Description,
                command.Quantity,
                moneyResult.Value);

        if (addItemResult.IsFailure)
        {
            return Result<AddPurchaseRequestItemResponse>.Failure(
                addItemResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        var itemId = addItemResult.Value;

        var item =
            purchaseRequest.Items.Single(
                x => x.Id == itemId);

        return Result<AddPurchaseRequestItemResponse>.Success(
            new AddPurchaseRequestItemResponse(
                item.Id.Value,
                item.Description,
                item.Quantity,
                item.UnitPrice.Amount,
                item.TotalPrice.Amount,
                purchaseRequest.TotalAmount,
                purchaseRequest.Currency.Code));
    }
}