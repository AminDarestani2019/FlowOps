namespace FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;

public sealed class CamundaOptions
{
    public const string SectionName = "Camunda";

    public string BaseUrl { get; init; } =
        "http://localhost:8080";

    public string PurchaseRequestApprovalProcessId { get; init; } =
        "purchase-request-approval";
}
