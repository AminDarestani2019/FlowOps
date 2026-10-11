namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.CreatePurchaseRequest;

public sealed record CreatePurchaseRequestCommand(
    Guid DepartmentId,
    string Currency);
