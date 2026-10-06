using FlowOps.Modules.Procurement.Domain.PurchaseRequests;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;
using FlowOps.Modules.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace FlowOps.Modules.Procurement.IntegrationTests.PurchaseRequests;

public sealed class PurchaseRequestPersistenceTests
{
    [Fact]
    public async Task PurchaseRequest_ShouldBePersistedAndLoadedCorrectly()
    {
        // Arrange
      await using var postgres =
        new PostgreSqlBuilder("postgres:17-alpine")
          .WithDatabase("flowops_tests")
          .WithUsername("flowops")
          .WithPassword("flowops_test_password")
          .Build();

        await postgres.StartAsync();

        var options =
            new DbContextOptionsBuilder<ProcurementDbContext>()
                .UseNpgsql(postgres.GetConnectionString())
                .Options;

        await using (var setupContext =
                     new ProcurementDbContext(options))
        {
            await setupContext.Database.MigrateAsync();
        }

        var numberResult =
            PurchaseRequestNumber.Create(
                "PR-2026-TEST-000001");

        Assert.True(numberResult.IsSuccess);

        var currencyResult =
            Currency.Create("EUR");

        Assert.True(currencyResult.IsSuccess);

        var requesterId =
            EmployeeId.New();

        var departmentId =
            DepartmentId.New();

        var createdAt =
            new DateTime(
                2026,
                10,
                6,
                8,
                0,
                0,
                DateTimeKind.Utc);

        var requestResult =
            PurchaseRequest.Create(
                numberResult.Value,
                requesterId,
                departmentId,
                currencyResult.Value,
                createdAt);

        Assert.True(requestResult.IsSuccess);

        var request =
            requestResult.Value;

        var moneyResult =
            Money.Create(
                1500m,
                currencyResult.Value);

        Assert.True(moneyResult.IsSuccess);

        var addItemResult =
            request.AddItem(
                "Development Laptop",
                2,
                moneyResult.Value);

        Assert.True(addItemResult.IsSuccess);

        var requestId =
            request.Id;

        // Act - Save
        await using (var writeContext =
                     new ProcurementDbContext(options))
        {
            writeContext.PurchaseRequests.Add(request);

            await writeContext.SaveChangesAsync();
        }

        // Act - Load using a completely new DbContext
        PurchaseRequest loadedRequest;

        await using (var readContext =
                     new ProcurementDbContext(options))
        {
            loadedRequest =
                await readContext.PurchaseRequests
                    .AsNoTracking()
                    .Include(x => x.Items)
                    .SingleAsync(
                        x => x.Id == requestId);
        }

        // Assert
        Assert.Equal(
            requestId,
            loadedRequest.Id);

        Assert.Equal(
            "PR-2026-TEST-000001",
            loadedRequest.RequestNumber.Value);

        Assert.Equal(
            requesterId,
            loadedRequest.RequesterId);

        Assert.Equal(
            departmentId,
            loadedRequest.DepartmentId);

        Assert.Equal(
            "EUR",
            loadedRequest.Currency.Code);

        Assert.Equal(
            PurchaseRequestStatus.Draft,
            loadedRequest.Status);

        Assert.Equal(
            createdAt,
            loadedRequest.CreatedAtUtc);

        Assert.Null(
            loadedRequest.SubmittedAtUtc);

        Assert.Equal(
            3000m,
            loadedRequest.TotalAmount);

        var loadedItem =
            Assert.Single(
                loadedRequest.Items);

        Assert.Equal(
            "Development Laptop",
            loadedItem.Description);

        Assert.Equal(
            2,
            loadedItem.Quantity);

        Assert.Equal(
            1500m,
            loadedItem.UnitPrice.Amount);

        Assert.Equal(
            "EUR",
            loadedItem.UnitPrice.Currency.Code);

        Assert.Equal(
            3000m,
            loadedItem.TotalPrice.Amount);
    }
}
