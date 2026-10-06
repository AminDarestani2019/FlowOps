using FlowOps.Modules.Procurement.Domain.PurchaseRequests;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowOps.Modules.Procurement.Infrastructure.Persistence.Configurations;

internal sealed class PurchaseRequestItemConfiguration
    : IEntityTypeConfiguration<PurchaseRequestItem>
{
    public void Configure(
        EntityTypeBuilder<PurchaseRequestItem> builder)
    {
        builder.ToTable(
            "purchase_request_items",
            "procurement");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new PurchaseRequestItemId(value))
            .ValueGeneratedNever();

        builder.Property<PurchaseRequestId>(
                "PurchaseRequestId")
            .HasColumnName("purchase_request_id")
            .HasConversion(
                id => id.Value,
                value => new PurchaseRequestId(value))
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.OwnsOne(
            x => x.UnitPrice,
            money =>
            {
                money.Property(x => x.Amount)
                    .HasColumnName("unit_price_amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(x => x.Currency)
                    .HasColumnName("unit_price_currency")
                    .HasMaxLength(3)
                    .HasConversion(
                        currency => currency.Code,
                        value => Currency.Create(value).Value)
                    .IsRequired();
            });

        builder.Navigation(x => x.UnitPrice)
            .IsRequired();
    }
}
