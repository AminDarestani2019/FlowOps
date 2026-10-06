using FlowOps.BuildingBlocks.Domain.Results;

namespace FlowOps.Modules.Procurement.Application.PurchaseRequests.CreatePurchaseRequest;

public static class CreatePurchaseRequestValidator
{
    public static Result Validate(
       CreatePurchaseRequestCommand command)
    {
        if (command.DepartmentId == Guid.Empty)
        {
            return Result.Failure(
                new Error(
                    "PurchaseRequest.DepartmentRequired",
                    "Department is required."));
        }

        if (string.IsNullOrWhiteSpace(command.Currency))
        {
            return Result.Failure(
                new Error(
                    "Currency.CodeRequired",
                    "Currency code is required."));
        }

        return Result.Success();
    }
}
