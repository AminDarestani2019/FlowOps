using FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.SubmitPurchaseRequest;

namespace FlowOps.Api.Endpoints.Procurement;

public static class SubmitPurchaseRequestEndpoint
{
    public static void Map(
        IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/procurement/purchase-requests/{purchaseRequestId:guid}/submit",
            HandleAsync)
            .WithName("SubmitPurchaseRequest")
            .WithTags("Procurement")
            .Produces<SubmitPurchaseRequestResponse>(
                StatusCodes.Status200OK)
            .ProducesProblem(
                StatusCodes.Status400BadRequest)
            .ProducesProblem(
                StatusCodes.Status404NotFound)
            .ProducesProblem(
                StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleAsync(
        Guid purchaseRequestId,
        SubmitPurchaseRequestHandler handler,
        CancellationToken cancellationToken)
    {
        var result =
            await handler.HandleAsync(
                new SubmitPurchaseRequestCommand(
                    purchaseRequestId),
                cancellationToken);

        if (result.IsSuccess)
        {
            return TypedResults.Ok(
                result.Value);
        }

        if (
            result.Error.Code ==
            "PurchaseRequest.NotFound"
        )
        {
            return TypedResults.Problem(
                statusCode:
                    StatusCodes.Status404NotFound,
                title:
                    result.Error.Code,
                detail:
                    result.Error.Description);
        }

        if (
            result.Error.Code ==
            "PurchaseRequest.InvalidStateTransition" ||
            result.Error.Code ==
            "PurchaseRequest.CannotModifyNonDraft"
        )
        {
            return TypedResults.Problem(
                statusCode:
                    StatusCodes.Status409Conflict,
                title:
                    result.Error.Code,
                detail:
                    result.Error.Description);
        }

        return TypedResults.Problem(
            statusCode:
                StatusCodes.Status400BadRequest,
            title:
                result.Error.Code,
            detail:
                result.Error.Description);
    }
}