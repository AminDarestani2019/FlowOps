using FlowOps.Modules.Procurement.Application.PurchaseRequests.Queries.GetMyPurchaseRequests;
using FlowOps.Modules.Procurement.Application.PurchaseRequests.Queries.Models;

namespace FlowOps.Api.Endpoints.Procurement;

public static class GetMyPurchaseRequestsEndpoint
{
    public static void Map(
        IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/procurement/purchase-requests/mine",
            HandleAsync)
            .WithName("GetMyPurchaseRequests")
            .WithTags("Procurement")
            .Produces<
                PagedResult<PurchaseRequestListItem>>(
                    StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        string? search,
        string? status,
        string? sortBy,
        bool? descending,
        int? page,
        int? pageSize,
        GetMyPurchaseRequestsHandler handler,
        CancellationToken cancellationToken)
    {
        var query =
            new GetMyPurchaseRequestsQuery(
                Search:
                    search,

                Status:
                    status,

                SortBy:
                    sortBy ?? "createdAt",

                Descending:
                    descending ?? true,

                Page:
                    page ?? 1,

                PageSize:
                    pageSize ?? 20);

        var result =
            await handler.HandleAsync(
                query,
                cancellationToken);

        return TypedResults.Ok(
            result);
    }
}
