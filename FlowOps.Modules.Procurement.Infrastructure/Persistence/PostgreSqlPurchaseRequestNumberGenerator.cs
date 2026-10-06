using FlowOps.BuildingBlocks.Domain.Time;
using FlowOps.Modules.Procurement.Application.Abstractions.Persistence;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FlowOps.Modules.Procurement.Infrastructure.Persistence;

public sealed class PostgreSqlPurchaseRequestNumberGenerator : IPurchaseRequestNumberGenerator
{
    private readonly ProcurementDbContext _dbContext;
    private readonly IClock _clock;

    public PostgreSqlPurchaseRequestNumberGenerator(
        ProcurementDbContext dbContext,
        IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<PurchaseRequestNumber> GenerateAsync(
        CancellationToken cancellationToken = default)
    {
        var sequence = await _dbContext.Database
            .SqlQueryRaw<long>(
                """
                SELECT nextval(
                    'procurement.purchase_request_number_seq'
                ) AS "Value"
                """)
            .SingleAsync(cancellationToken);

        var number = $"PR-{_clock.UtcNow.Year}-{sequence:D6}";

        return PurchaseRequestNumber.Create(number).Value;
    }
}
