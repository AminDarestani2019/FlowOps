using FlowOps.Modules.Procurement.Domain.PurchaseRequests;

namespace FlowOps.Modules.Procurement.Application.Abstractions.Persistence;

public interface IProcurementUnitOfWork
{
    void Add(PurchaseRequest purchaseRequest);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
