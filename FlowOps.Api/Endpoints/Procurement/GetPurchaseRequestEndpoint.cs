using FlowOps.Modules.Procurement.Application.Queries.GetPurchaseRequest;
using FlowOps.Modules.Procurement.Application.Queries.Models;

namespace FlowOps.Api.Endpoints.Procurement;

public static class GetPurchaseRequestEndpoint
{
    public static void Map(
        IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/procurement/purchase-requests/{purchaseRequestId:guid}",
            HandleAsync)
            .WithName("GetPurchaseRequest")
            .WithTags("Procurement")
            .Produces<PurchaseRequestDetails>(
                StatusCodes.Status200OK)
            .ProducesProblem(
                StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid purchaseRequestId,
        GetPurchaseRequestHandler handler,
        CancellationToken cancellationToken)
    {
        var result =
            await handler.HandleAsync(
                new GetPurchaseRequestQuery(
                    purchaseRequestId),
                cancellationToken);

        if (result.IsFailure)
        {
            return TypedResults.Problem(
                statusCode:
                    StatusCodes.Status404NotFound,
                title:
                    result.Error.Code,
                detail:
                    result.Error.Description);
        }

        return TypedResults.Ok(
            result.Value);
    }
}
