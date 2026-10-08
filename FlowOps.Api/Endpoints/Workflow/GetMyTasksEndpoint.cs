using FlowOps.Modules.Procurement.Application.Features.Workflow.GetMyTasks;

namespace FlowOps.Api.Endpoints.Workflow;

public static class GetMyTasksEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet(
            "/api/workflow/tasks",
            async (
                GetMyTasksHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new GetMyTasksQuery(),
                    cancellationToken);

                return Results.Ok(result);
            });
    }
}