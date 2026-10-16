using FlowOps.Modules.Procurement.Application.Abstractions.Identity;
using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

namespace FlowOps.Api.Development;

public sealed class DevelopmentWorkflowCurrentUser(
    IHttpContextAccessor httpContextAccessor)
    : IWorkflowCurrentUser
{
    private const string UserHeader =
        "X-FlowOps-User";

    private const string GroupsHeader =
        "X-FlowOps-Groups";

    private const string DefaultUserId =
        "flowops-dev-manager";

    public string UserId
    {
        get
        {
            var value =
                httpContextAccessor
                    .HttpContext?
                    .Request
                    .Headers[UserHeader]
                    .FirstOrDefault();

            return string.IsNullOrWhiteSpace(value)
                ? DefaultUserId
                : value.Trim();
        }
    }

    public IReadOnlyCollection<string> CandidateGroups
    {
        get
        {
            var value =
                httpContextAccessor
                    .HttpContext?
                    .Request
                    .Headers[GroupsHeader]
                    .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(value))
            {
                return new[]
                {
                    WorkflowCandidateGroups.ProcurementManagers
                };
            }

            return value
                .Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }
    }
}
