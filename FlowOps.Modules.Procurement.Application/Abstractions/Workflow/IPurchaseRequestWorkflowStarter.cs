namespace FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

public interface IPurchaseRequestWorkflowStarter
{
    Task<StartPurchaseRequestWorkflowResult> StartAsync(
    StartPurchaseRequestWorkflowRequest request,
    CancellationToken cancellationToken = default);
}
