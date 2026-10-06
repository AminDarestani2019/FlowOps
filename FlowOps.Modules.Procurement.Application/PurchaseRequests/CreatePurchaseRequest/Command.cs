namespace FlowOps.Modules.Procurement.Application.PurchaseRequests.CreatePurchaseRequest;

public sealed record CreatePurchaseRequestCommand(
    Guid DepartmentId,
    string Currency);
