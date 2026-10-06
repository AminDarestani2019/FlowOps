using FlowOps.BuildingBlocks.Domain.Primitives;
using FlowOps.BuildingBlocks.Domain.Results;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.Errors;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.Events;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;

namespace FlowOps.Modules.Procurement.Domain.PurchaseRequests;

public sealed class PurchaseRequest
    : AggregateRoot<PurchaseRequestId>
{
    private readonly List<PurchaseRequestItem> _items = [];

    private PurchaseRequest(
        PurchaseRequestId id,
        PurchaseRequestNumber requestNumber,
        EmployeeId requesterId,
        DepartmentId departmentId,
        Currency currency,
        DateTime createdAtUtc)
        : base(id)
    {
        RequestNumber = requestNumber;
        RequesterId = requesterId;
        DepartmentId = departmentId;
        Currency = currency;
        Status = PurchaseRequestStatus.Draft;
        CreatedAtUtc = createdAtUtc;
    }

    private PurchaseRequest()
    {
        RequestNumber = null!;
        Currency = null!;
    }

    public PurchaseRequestNumber RequestNumber { get; private set; }

    public EmployeeId RequesterId { get; private set; }

    public DepartmentId DepartmentId { get; private set; }

    public Currency Currency { get; private set; }

    public PurchaseRequestStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? SubmittedAtUtc { get; private set; }

    public IReadOnlyCollection<PurchaseRequestItem> Items =>
        _items.AsReadOnly();

    public decimal TotalAmount =>
        _items.Sum(item => item.TotalPrice.Amount);

    public static Result<PurchaseRequest> Create(
        PurchaseRequestNumber requestNumber,
        EmployeeId requesterId,
        DepartmentId departmentId,
        Currency currency,
        DateTime createdAtUtc)
    {
        if (requesterId.Value == Guid.Empty)
        {
            return Result<PurchaseRequest>.Failure(
                PurchaseRequestErrors.RequesterRequired);
        }

        if (departmentId.Value == Guid.Empty)
        {
            return Result<PurchaseRequest>.Failure(
                PurchaseRequestErrors.DepartmentRequired);
        }

        var purchaseRequest = new PurchaseRequest(
            PurchaseRequestId.New(),
            requestNumber,
            requesterId,
            departmentId,
            currency,
            createdAtUtc);

        purchaseRequest.RaiseDomainEvent(
            new PurchaseRequestCreatedDomainEvent(
                purchaseRequest.Id,
                requesterId,
                departmentId,
                createdAtUtc));

        return Result<PurchaseRequest>.Success(
            purchaseRequest);
    }

    public Result<PurchaseRequestItemId> AddItem(
        string description,
        int quantity,
        Money unitPrice)
    {
        var stateCheck = EnsureDraft();

        if (stateCheck.IsFailure)
        {
            return Result<PurchaseRequestItemId>.Failure(
                stateCheck.Error);
        }

        if (unitPrice.Currency != Currency)
        {
            return Result<PurchaseRequestItemId>.Failure(
                PurchaseRequestErrors.CurrencyMismatch);
        }

        var itemResult = PurchaseRequestItem.Create(
            description,
            quantity,
            unitPrice);

        if (itemResult.IsFailure)
        {
            return Result<PurchaseRequestItemId>.Failure(
                itemResult.Error);
        }

        var item = itemResult.Value;

        _items.Add(item);

        return Result<PurchaseRequestItemId>.Success(
            item.Id);
    }

    public Result UpdateItem(
        PurchaseRequestItemId itemId,
        string description,
        int quantity,
        Money unitPrice)
    {
        var stateCheck = EnsureDraft();

        if (stateCheck.IsFailure)
        {
            return stateCheck;
        }

        if (unitPrice.Currency != Currency)
        {
            return Result.Failure(
                PurchaseRequestErrors.CurrencyMismatch);
        }

        var item = _items.FirstOrDefault(
            x => x.Id == itemId);

        if (item is null)
        {
            return Result.Failure(
                new Error(
                    "PurchaseRequest.ItemNotFound",
                    "Purchase request item was not found."));
        }

        return item.Update(
            description,
            quantity,
            unitPrice);
    }

    public Result RemoveItem(
        PurchaseRequestItemId itemId)
    {
        var stateCheck = EnsureDraft();

        if (stateCheck.IsFailure)
        {
            return stateCheck;
        }

        var item = _items.FirstOrDefault(
            x => x.Id == itemId);

        if (item is null)
        {
            return Result.Failure(
                new Error(
                    "PurchaseRequest.ItemNotFound",
                    "Purchase request item was not found."));
        }

        _items.Remove(item);

        return Result.Success();
    }

    public Result Submit(DateTime submittedAtUtc)
    {
        if (Status != PurchaseRequestStatus.Draft)
        {
            return Result.Failure(
                PurchaseRequestErrors.InvalidStateTransition);
        }

        if (_items.Count == 0)
        {
            return Result.Failure(
                PurchaseRequestErrors.EmptyRequest);
        }

        Status = PurchaseRequestStatus.Submitted;
        SubmittedAtUtc = submittedAtUtc;

        return Result.Success();
    }

    private Result EnsureDraft()
    {
        return Status == PurchaseRequestStatus.Draft
            ? Result.Success()
            : Result.Failure(
                PurchaseRequestErrors.CannotModifyNonDraft);
    }
}