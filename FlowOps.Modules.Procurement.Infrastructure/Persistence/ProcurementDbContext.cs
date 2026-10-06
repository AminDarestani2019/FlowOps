using FlowOps.Modules.Procurement.Domain.PurchaseRequests;
using Microsoft.EntityFrameworkCore;

namespace FlowOps.Modules.Procurement.Infrastructure.Persistence;

public sealed class ProcurementDbContext
    : DbContext
{
    public ProcurementDbContext(
        DbContextOptions<ProcurementDbContext> options)
        : base(options)
    {
    }

    public DbSet<PurchaseRequest> PurchaseRequests =>
        Set<PurchaseRequest>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .HasSequence<long>(
                "purchase_request_number_seq",
                schema: "procurement")
            .StartsAt(1)
            .IncrementsBy(1);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ProcurementDbContext).Assembly);
    }
}