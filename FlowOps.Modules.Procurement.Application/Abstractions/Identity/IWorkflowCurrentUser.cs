namespace FlowOps.Modules.Procurement.Application.Abstractions.Identity;

public interface IWorkflowCurrentUser
{
    string UserId { get; }

    IReadOnlyCollection<string> CandidateGroups { get; }
}
