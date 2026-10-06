using FlowOps.Modules.Procurement.Application.Abstractions.Persistence;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests;

namespace FlowOps.Modules.Procurement.Infrastructure.Persistence;

public sealed class ProcurementUnitOfWork : IProcurementUnitOfWork
{
    private readonly ProcurementDbContext _dbContext;

    public ProcurementUnitOfWork(
        ProcurementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(PurchaseRequest purchaseRequest)
    {
        _dbContext.PurchaseRequests.Add(purchaseRequest);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
