namespace FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

public static class WorkflowTaskVisibilityPolicy
{
    public static bool IsVisibleTo(
        WorkflowTask task,
        string userId,
        IReadOnlyCollection<string> userGroups)
    {
        if (!string.IsNullOrWhiteSpace(task.Assignee))
        {
            return string.Equals(
                task.Assignee,
                userId,
                StringComparison.Ordinal);
        }

        var candidateUser =
            task.CandidateUsers.Any(
                candidate =>
                    string.Equals(
                        candidate,
                        userId,
                        StringComparison.Ordinal));

        if (candidateUser)
        {
            return true;
        }

        return task.CandidateGroups.Any(
            candidateGroup =>
                userGroups.Contains(
                    candidateGroup,
                    StringComparer.Ordinal));
    }
}
