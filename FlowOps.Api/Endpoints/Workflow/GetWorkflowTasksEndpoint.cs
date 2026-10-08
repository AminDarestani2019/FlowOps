using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

namespace FlowOps.Api.Endpoints.Workflow;

public static class GetWorkflowTasksEndpoint
{
    public static IEndpointRouteBuilder MapGetWorkflowTasks(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/workflow/tasks",
            async (
                IWorkflowTaskReader taskReader,
                CancellationToken cancellationToken) =>
            {
                var tasks =
                    await taskReader.GetActiveTasksAsync(cancellationToken);

                return Results.Ok(tasks);
            });

        return app;
    }
}
