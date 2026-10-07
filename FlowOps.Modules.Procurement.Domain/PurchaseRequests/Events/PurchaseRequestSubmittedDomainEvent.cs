using FlowOps.BuildingBlocks.Domain.Events;

namespace FlowOps.Modules.Procurement.Domain.PurchaseRequests.Events;

public sealed record PurchaseRequestSubmittedDomainEvent(
    PurchaseRequestId PurchaseRequestId,
    EmployeeId RequesterId,
    DepartmentId DepartmentId,
    DateTime OccurredOnUtc)
    : IDomainEvent;
