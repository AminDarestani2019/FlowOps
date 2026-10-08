using FlowOps.BuildingBlocks.Domain.Events;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests;
using FlowOps.Modules.Procurement.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FlowOps.Modules.Procurement.Infrastructure.Persistence;

public sealed class ProcurementDbContext
    : DbContext
{
    public ProcurementDbContext(
        DbContextOptions<ProcurementDbContext> options)
        : base(options)
    {
    }

    internal DbSet<OutboxMessage> OutboxMessages =>
        Set<OutboxMessage>();

    private static readonly JsonSerializerOptions OutboxJsonOptions =  new(JsonSerializerDefaults.Web);

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

    public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
    {
        var aggregates = ChangeTracker
            .Entries()
            .Select(entry => entry.Entity)
            .OfType<IHasDomainEvents>()
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = aggregates
            .SelectMany(aggregate => aggregate.DomainEvents)
            .ToList();

        var outboxMessages = domainEvents
            .Select(domainEvent =>
            {
                var eventType = domainEvent.GetType();

                var type =
                    eventType.FullName
                    ?? eventType.Name;

                var content =
                    JsonSerializer.Serialize(
                        domainEvent,
                        eventType,
                        OutboxJsonOptions);

                return new OutboxMessage(
                    Guid.NewGuid(),
                    domainEvent.OccurredOnUtc,
                    type,
                    content);
            })
            .ToList();

        if (outboxMessages.Count > 0)
        {
            OutboxMessages.AddRange(outboxMessages);
        }

        try
        {
            var result =
                await base.SaveChangesAsync(
                    cancellationToken);

            foreach (var aggregate in aggregates)
            {
                aggregate.ClearDomainEvents();
            }

            return result;
        }
        catch
        {
            foreach (var message in outboxMessages)
            {
                Entry(message).State =
                    EntityState.Detached;
            }

            throw;
        }
    }
}