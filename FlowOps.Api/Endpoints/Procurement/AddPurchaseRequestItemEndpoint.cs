using FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.AddPurchaseRequestItem;

namespace FlowOps.Api.Endpoints.Procurement;

public static class AddPurchaseRequestItemEndpoint
{
    public sealed record Request(
        string Description,
        int Quantity,
        decimal UnitPrice);

    public static void Map(
        IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/procurement/purchase-requests/{purchaseRequestId:guid}/items",
            HandleAsync)
            .WithName("AddPurchaseRequestItem")
            .WithTags("Procurement")
            .Produces<AddPurchaseRequestItemResponse>(
                StatusCodes.Status201Created)
            .ProducesProblem(
                StatusCodes.Status400BadRequest)
            .ProducesProblem(
                StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid purchaseRequestId,
        Request request,
        AddPurchaseRequestItemHandler handler,
        CancellationToken cancellationToken)
    {
        var command =
            new AddPurchaseRequestItemCommand(
                purchaseRequestId,
                request.Description,
                request.Quantity,
                request.UnitPrice);

        var result =
            await handler.HandleAsync(
                command,
                cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code ==
                "PurchaseRequest.NotFound")
            {
                return TypedResults.Problem(
                    statusCode:
                        StatusCodes.Status404NotFound,
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

        var response = result.Value;

        return TypedResults.Created(
            $"/api/procurement/purchase-requests/{purchaseRequestId}/items/{response.ItemId}",
            response);
    }
}