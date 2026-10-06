using FlowOps.BuildingBlocks.Domain.Results;
using FlowOps.BuildingBlocks.Domain.Time;
using FlowOps.Modules.Procurement.Application.Abstractions.Identity;
using FlowOps.Modules.Procurement.Application.Abstractions.Persistence;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;

namespace FlowOps.Modules.Procurement.Application.PurchaseRequests.CreatePurchaseRequest;

public sealed class CreatePurchaseRequestHandler
{
    private readonly IProcurementUnitOfWork _unitOfWork;
    private readonly IPurchaseRequestNumberGenerator _numberGenerator;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public CreatePurchaseRequestHandler(
        IProcurementUnitOfWork unitOfWork,
        IPurchaseRequestNumberGenerator numberGenerator,
        IClock clock,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _numberGenerator = numberGenerator;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<Result<CreatePurchaseRequestResponse>> HandleAsync(
        CreatePurchaseRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult =
            CreatePurchaseRequestValidator.Validate(command);

        if (validationResult.IsFailure)
        {
            return Result<CreatePurchaseRequestResponse>.Failure(
                validationResult.Error);
        }

        var currencyResult = Currency.Create(command.Currency);

        if (currencyResult.IsFailure)
        {
            return Result<CreatePurchaseRequestResponse>.Failure(
                currencyResult.Error);
        }

        var requestNumber =
            await _numberGenerator.GenerateAsync(cancellationToken);

        var purchaseRequestResult = PurchaseRequest.Create(
            requestNumber,
            _currentUser.EmployeeId,
            new DepartmentId(command.DepartmentId),
            currencyResult.Value,
            _clock.UtcNow);

        if (purchaseRequestResult.IsFailure)
        {
            return Result<CreatePurchaseRequestResponse>.Failure(
                purchaseRequestResult.Error);
        }

        var purchaseRequest = purchaseRequestResult.Value;

        _unitOfWork.Add(purchaseRequest);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreatePurchaseRequestResponse>.Success(
            new CreatePurchaseRequestResponse(
                purchaseRequest.Id.Value,
                purchaseRequest.RequestNumber.Value,
                purchaseRequest.Status.ToString()));
    }
}
