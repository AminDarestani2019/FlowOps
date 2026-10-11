using FlowOps.Modules.Procurement.Application.Features.Workflow.CompleteTask;

namespace FlowOps.Api.Endpoints.Workflow;

public static class CompleteTaskEndpoint
{
    public sealed record Request(
        string? Action,
        Dictionary<string, object?>? Variables);

    public static void Map(WebApplication app)
    {
        app.MapPost(
            "/api/workflow/tasks/{taskKey}/complete",
            async (
                string taskKey,
                Request request,
                Handler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new Command(
                        taskKey,
                        request.Action,
                        request.Variables),
                    cancellationToken);

                return result.Completed
                    ? Results.NoContent()
                    : Results.NotFound();
            });
    }
}
