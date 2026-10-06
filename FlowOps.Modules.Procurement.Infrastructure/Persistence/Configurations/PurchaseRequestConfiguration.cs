using FlowOps.Modules.Procurement.Domain.PurchaseRequests;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowOps.Modules.Procurement.Infrastructure.Persistence.Configurations;

internal sealed class PurchaseRequestConfiguration
    : IEntityTypeConfiguration<PurchaseRequest>
{
    public void Configure(
        EntityTypeBuilder<PurchaseRequest> builder)
    {
        builder.ToTable(
            "purchase_requests",
            "procurement");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new PurchaseRequestId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.RequestNumber)
            .HasColumnName("request_number")
            .HasMaxLength(50)
            .HasConversion(
                number => number.Value,
                value => PurchaseRequestNumber
                    .Create(value)
                    .Value)
            .IsRequired();

        builder.HasIndex(x => x.RequestNumber)
            .IsUnique();

        builder.Property(x => x.RequesterId)
            .HasColumnName("requester_id")
            .HasConversion(
                id => id.Value,
                value => new EmployeeId(value))
            .IsRequired();

        builder.Property(x => x.DepartmentId)
            .HasColumnName("department_id")
            .HasConversion(
                id => id.Value,
                value => new DepartmentId(value))
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasConversion(
                currency => currency.Code,
                value => Currency.Create(value).Value)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.SubmittedAtUtc)
            .HasColumnName("submitted_at_utc");

        builder.Ignore(x => x.TotalAmount);

        builder.Ignore(x => x.DomainEvents);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey("PurchaseRequestId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items)
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);
    }
}
