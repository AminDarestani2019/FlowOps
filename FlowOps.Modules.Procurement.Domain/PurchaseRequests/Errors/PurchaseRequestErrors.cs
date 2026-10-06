using FlowOps.BuildingBlocks.Domain.Results;

namespace FlowOps.Modules.Procurement.Domain.PurchaseRequests.Errors;

public static class PurchaseRequestErrors
{
    public static readonly Error RequesterRequired =
        new(
            "PurchaseRequest.RequesterRequired",
            "Requester is required.");

    public static readonly Error DepartmentRequired =
        new(
            "PurchaseRequest.DepartmentRequired",
            "Department is required.");

    public static readonly Error DescriptionRequired =
        new(
            "PurchaseRequest.DescriptionRequired",
            "Item description is required.");

    public static readonly Error InvalidQuantity =
        new(
            "PurchaseRequest.InvalidQuantity",
            "Item quantity must be greater than zero.");

    public static readonly Error CannotModifyNonDraft =
        new(
            "PurchaseRequest.CannotModifyNonDraft",
            "Only draft purchase requests can be modified.");

    public static readonly Error EmptyRequest =
        new(
            "PurchaseRequest.EmptyRequest",
            "A purchase request must contain at least one item before submission.");

    public static readonly Error InvalidStateTransition =
        new(
            "PurchaseRequest.InvalidStateTransition",
            "The requested state transition is not allowed.");

    public static readonly Error CurrencyMismatch =
        new(
            "PurchaseRequest.CurrencyMismatch",
            "All purchase request items must use the same currency.");
}