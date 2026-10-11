using FlowOps.BuildingBlocks.Domain.Events;

namespace FlowOps.Modules.Procurement.Domain.PurchaseRequests.Events;

public sealed record PurchaseRequestApprovedDomainEvent(
    PurchaseRequestId PurchaseRequestId,
    DateTime OccurredOnUtc)
    : IDomainEvent;
