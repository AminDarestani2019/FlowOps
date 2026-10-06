using FlowOps.Modules.Procurement.Application.Abstractions.Persistence;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests;
using Microsoft.EntityFrameworkCore;

namespace FlowOps.Modules.Procurement.Infrastructure.Persistence;

public sealed class PurchaseRequestRepository
    : IPurchaseRequestRepository
{
    private readonly ProcurementDbContext _dbContext;

    public PurchaseRequestRepository(
        ProcurementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PurchaseRequest?> GetByIdAsync(
        PurchaseRequestId id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PurchaseRequests
            .Include(x => x.Items)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }
}