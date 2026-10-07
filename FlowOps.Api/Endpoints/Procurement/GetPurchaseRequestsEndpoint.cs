using FlowOps.Modules.Procurement.Application.PurchaseRequests.Queries.GetPurchaseRequests;
using FlowOps.Modules.Procurement.Application.PurchaseRequests.Queries.Models;
using FlowOps.Modules.Procurement.Application.Queries.GetPurchaseRequests;

namespace FlowOps.Api.Endpoints.Procurement;

public static class GetPurchaseRequestsEndpoint
{
    public static void Map(
        IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/procurement/purchase-requests",
            HandleAsync)
            .WithName("GetPurchaseRequests")
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
        GetPurchaseRequestsHandler handler,
        CancellationToken cancellationToken)
    {
        var query =
            new GetPurchaseRequestsQuery(
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

        return TypedResults.Ok(result);
    }
}
