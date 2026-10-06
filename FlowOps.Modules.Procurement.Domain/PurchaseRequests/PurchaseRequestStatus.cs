namespace FlowOps.Modules.Procurement.Domain.PurchaseRequests;

public enum PurchaseRequestStatus
{
    Draft = 1,
    Submitted = 2,
    InApproval = 3,
    Approved = 4,
    Rejected = 5,
    Completed = 6,
    Cancelled = 7
}
