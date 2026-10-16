namespace FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

public static class WorkflowTaskSlaStatusResolver
{
    public const string Due = "Due";
    public const string Overdue = "Overdue";
    public const string Escalated = "Escalated";

    private const string ManagerApprovalElementId =
        "UserTask_ManagerApproval";

    public static string? Resolve(
        WorkflowTask task,
        DateTimeOffset utcNow,
        bool managerApprovalEscalated)
    {
        if (
            managerApprovalEscalated &&
            string.Equals(
                task.ElementId,
                ManagerApprovalElementId,
                StringComparison.Ordinal)
        )
        {
            return Escalated;
        }

        if (task.DueDate is null)
        {
            return null;
        }

        return task.DueDate.Value <= utcNow
            ? Overdue
            : Due;
    }
}
