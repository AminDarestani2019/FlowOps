using FlowOps.Modules.Procurement.Domain.PurchaseRequests;

namespace FlowOps.Modules.Procurement.Application.Abstractions.Persistence;

public interface IPurchaseRequestRepository
{
    Task<PurchaseRequest?> GetByIdAsync(
        PurchaseRequestId id,
        CancellationToken cancellationToken = default);
}