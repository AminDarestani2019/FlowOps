using FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.AcknowledgePurchaseRequest;

namespace FlowOps.Api.Endpoints.Procurement;

public static class AcknowledgePurchaseRequestEndpoint
{
    public static void Map(
        IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/procurement/purchase-requests/{purchaseRequestId:guid}/acknowledge",
            HandleAsync)
            .WithName("AcknowledgePurchaseRequest")
            .WithTags("Procurement")
            .Produces<AcknowledgePurchaseRequestResponse>(
                StatusCodes.Status200OK)
            .ProducesProblem(
                StatusCodes.Status404NotFound)
            .ProducesProblem(
                StatusCodes.Status409Conflict)
            .ProducesProblem(
                StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        Guid purchaseRequestId,
        AcknowledgePurchaseRequestHandler handler,
        CancellationToken cancellationToken)
    {
        var result =
            await handler.HandleAsync(
                new AcknowledgePurchaseRequestCommand(
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
                "Workflow.MessageNotCorrelated"
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