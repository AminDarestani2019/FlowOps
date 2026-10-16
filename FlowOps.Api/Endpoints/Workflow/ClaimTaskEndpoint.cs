using FlowOps.Modules.Procurement.Application.Features.Workflow.ClaimTask;

namespace FlowOps.Api.Endpoints.Workflow;

public static class ClaimTaskEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost(
            "/api/workflow/tasks/{taskKey}/claim",
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
                    ClaimTaskStatus.Claimed =>
                        Results.NoContent(),

                    ClaimTaskStatus.NotFound =>
                        Results.NotFound(),

                    ClaimTaskStatus.Forbidden =>
                        Results.StatusCode(
                            StatusCodes.Status403Forbidden),

                    ClaimTaskStatus.Conflict =>
                        Results.Conflict(),

                    _ =>
                        Results.StatusCode(
                            StatusCodes.Status500InternalServerError)
                };
            });
    }
}
