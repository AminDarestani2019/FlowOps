using FlowOps.BuildingBlocks.Domain.Events;

namespace FlowOps.Modules.Procurement.Domain.PurchaseRequests.Events;

public sealed record PurchaseRequestRejectedDomainEvent(
    PurchaseRequestId PurchaseRequestId,
    DateTime OccurredOnUtc)
    : IDomainEvent;