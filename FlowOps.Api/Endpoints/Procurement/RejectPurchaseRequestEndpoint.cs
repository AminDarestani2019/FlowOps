using FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.RejectPurchaseRequest;

namespace FlowOps.Api.Endpoints.Procurement;

public static class RejectPurchaseRequestEndpoint
{
    public static void Map(
        IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/procurement/purchase-requests/{purchaseRequestId:guid}/reject",
            HandleAsync)
            .WithName("RejectPurchaseRequest")
            .WithTags("Procurement")
            .Produces<RejectPurchaseRequestResponse>(
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
        RejectPurchaseRequestRequest request,
        RejectPurchaseRequestHandler handler,
        CancellationToken cancellationToken)
    {
        var result =
            await handler.HandleAsync(
                new RejectPurchaseRequestCommand(
                    purchaseRequestId,
                    request.TaskKey),
                cancellationToken);

        if (result.IsSuccess)
        {
            return TypedResults.Ok(
                result.Value);
        }

        if (
            result.Error.Code ==
                "PurchaseRequest.NotFound" ||
            result.Error.Code ==
                "Workflow.TaskNotFound" ||
            result.Error.Code ==
                "Workflow.ProcessNotFound"
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
                "Workflow.CorrelationMismatch" ||
            result.Error.Code ==
                "Workflow.TaskNotActive"
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

public sealed record RejectPurchaseRequestRequest(
    string TaskKey);