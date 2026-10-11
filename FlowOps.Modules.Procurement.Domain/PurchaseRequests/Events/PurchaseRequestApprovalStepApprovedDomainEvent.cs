using FlowOps.BuildingBlocks.Domain.Events;

namespace FlowOps.Modules.Procurement.Domain.PurchaseRequests.Events;

public sealed record PurchaseRequestApprovalStepApprovedDomainEvent(
    PurchaseRequestId PurchaseRequestId,
    PurchaseRequestApprovalStage Stage,
    bool IsFinalApproval,
    DateTime OccurredOnUtc)
    : IDomainEvent;