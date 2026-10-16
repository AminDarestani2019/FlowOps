using FlowOps.Modules.Procurement.Application.Features.Workflow.UnclaimTask;

namespace FlowOps.Api.Endpoints.Workflow;

public static class UnclaimTaskEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost(
            "/api/workflow/tasks/{taskKey}/unclaim",
            async (
                string taskKey,
                Handler handler,
                CancellationToken cancellationToken) =>
            {
                var result =
                    await handler.HandleAsync(
                        new Command(taskKey),
                        cancellationToken);

                return result.Status switch
                {
                    UnclaimTaskStatus.Unclaimed =>
                        Results.NoContent(),

                    UnclaimTaskStatus.NotFound =>
                        Results.NotFound(),

                    UnclaimTaskStatus.Forbidden =>
                        Results.StatusCode(
                            StatusCodes.Status403Forbidden),

                    UnclaimTaskStatus.Conflict =>
                        Results.Conflict(),

                    _ =>
                        Results.StatusCode(
                            StatusCodes.Status500InternalServerError)
                };
            });
    }
}
