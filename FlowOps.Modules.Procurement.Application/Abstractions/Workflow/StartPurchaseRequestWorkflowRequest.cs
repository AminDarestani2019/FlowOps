namespace FlowOps.Modules.Procurement.Application.Abstractions.Workflow;

public sealed record StartPurchaseRequestWorkflowRequest(
    Guid PurchaseRequestId,
    Guid RequesterId,
    Guid DepartmentId,
    decimal TotalAmount);
