using FlowOps.Modules.Procurement.Application.Features.Workflow.GetTaskDetails;

namespace FlowOps.Api.Endpoints.Workflow;

public static class GetTaskDetailsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet(
            "/api/workflow/tasks/{taskKey}",
            async (
                string taskKey,
                Handler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new Query(taskKey),
                    cancellationToken);

                return result is null
                    ? Results.NotFound()
                    : Results.Ok(result);
            });
    }
}
