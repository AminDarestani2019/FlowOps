using FlowOps.BuildingBlocks.Domain.Results;

namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.AddPurchaseRequestItem;

public static class AddPurchaseRequestItemValidator
{
    public static Result Validate(
        AddPurchaseRequestItemCommand command)
    {
        if (command.PurchaseRequestId == Guid.Empty)
        {
            return Result.Failure(
                new Error(
                    "PurchaseRequest.IdRequired",
                    "Purchase request ID is required."));
        }

        if (string.IsNullOrWhiteSpace(command.Description))
        {
            return Result.Failure(
                new Error(
                    "PurchaseRequestItem.DescriptionRequired",
                    "Item description is required."));
        }

        if (command.Quantity <= 0)
        {
            return Result.Failure(
                new Error(
                    "PurchaseRequestItem.InvalidQuantity",
                    "Quantity must be greater than zero."));
        }

        if (command.UnitPrice < 0)
        {
            return Result.Failure(
                new Error(
                    "PurchaseRequestItem.InvalidUnitPrice",
                    "Unit price cannot be negative."));
        }

        return Result.Success();
    }
}