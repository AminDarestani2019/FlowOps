using FlowOps.Modules.Procurement.Application.PurchaseRequests.CreatePurchaseRequest;

namespace FlowOps.Api.Endpoints.Procurement;

public static class CreatePurchaseRequestEndpoint
{
    public sealed record Request(
       Guid DepartmentId,
       string Currency);

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/procurement/purchase-requests",
            HandleAsync)
            .WithName("CreatePurchaseRequest")
            .WithTags("Procurement")
            .Produces<CreatePurchaseRequestResponse>(
                StatusCodes.Status201Created)
            .ProducesProblem(
                StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        CreatePurchaseRequestHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreatePurchaseRequestCommand(
            request.DepartmentId,
            request.Currency);

        var result = await handler.HandleAsync(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: result.Error.Code,
                detail: result.Error.Description);
        }

        var response = result.Value;

        return TypedResults.Created(
            $"/api/procurement/purchase-requests/{response.Id}",
            response);
    }
}
