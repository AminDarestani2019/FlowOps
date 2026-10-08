using FlowOps.Modules.Procurement.Domain.PurchaseRequests;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;
using FlowOps.Modules.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Testcontainers.PostgreSql;

namespace FlowOps.Modules.Procurement.IntegrationTests.PurchaseRequests;

public sealed class PurchaseRequestOutboxTests
{
    [Fact]
    public async Task SaveChangesAsync_WhenPurchaseRequestIsSubmitted_ShouldPersistOutboxMessage()
    {
        // Arrange
        await using var postgres =
            new PostgreSqlBuilder()
                .WithImage("postgres:17-alpine")
                .Build();

        await postgres.StartAsync();

        var options =
            new DbContextOptionsBuilder<ProcurementDbContext>()
                .UseNpgsql(postgres.GetConnectionString())
                .Options;

        await using (var migrationContext =
                     new ProcurementDbContext(options))
        {
            await migrationContext.Database.MigrateAsync();
        }

        var numberResult =
            PurchaseRequestNumber.Create(
                "PR-2026-000099");

        Assert.True(numberResult.IsSuccess);

        var currencyResult =
            Currency.Create("EUR");

        Assert.True(currencyResult.IsSuccess);

        var createResult =
            PurchaseRequest.Create(
                numberResult.Value,
                EmployeeId.New(),
                DepartmentId.New(),
                currencyResult.Value,
                new DateTime(
                    2026,
                    10,
                    8,
                    8,
                    0,
                    0,
                    DateTimeKind.Utc));

        Assert.True(createResult.IsSuccess);

        var request = createResult.Value;

        var moneyResult =
            Money.Create(
                1500m,
                currencyResult.Value);

        Assert.True(moneyResult.IsSuccess);

        var addItemResult =
            request.AddItem(
                "Outbox Integration Test Laptop",
                2,
                moneyResult.Value);

        Assert.True(addItemResult.IsSuccess);

        var submittedAtUtc =
            new DateTime(
                2026,
                10,
                8,
                9,
                0,
                0,
                DateTimeKind.Utc);

        var submitResult =
            request.Submit(submittedAtUtc);

        Assert.True(submitResult.IsSuccess);


        // Act
        await using (var context =
                     new ProcurementDbContext(options))
        {
            context.Set<PurchaseRequest>()
                .Add(request);

            await context.SaveChangesAsync();
        }


        // Assert - business state was committed
        await using (var verificationContext =
                     new ProcurementDbContext(options))
        {
            var persistedRequest =
                await verificationContext
                    .Set<PurchaseRequest>()
                    .AsNoTracking()
                    .SingleAsync(
                        x => x.Id == request.Id);

            Assert.Equal(
                PurchaseRequestStatus.Submitted,
                persistedRequest.Status);

            Assert.Equal(
                submittedAtUtc,
                persistedRequest.SubmittedAtUtc);
        }


        // Assert - Submitted domain event was persisted to Outbox
        await using (var outboxContext =
                     new ProcurementDbContext(options))
        {
            var connection =
                outboxContext.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                SELECT
                    type,
                    content::text,
                    processed_on_utc,
                    attempt_count,
                    last_error
                FROM procurement.outbox_messages
                WHERE type LIKE '%PurchaseRequestSubmittedDomainEvent%'
                ORDER BY occurred_on_utc DESC
                LIMIT 1;
                """;

            await using var reader =
                await command.ExecuteReaderAsync();

            Assert.True(
                await reader.ReadAsync(),
                "PurchaseRequestSubmittedDomainEvent was not found in the Outbox.");

            var type =
                reader.GetString(0);

            var content =
                reader.GetString(1);

            var processedOnUtcIsNull =
                reader.IsDBNull(2);

            var attemptCount =
                reader.GetInt32(3);

            var lastErrorIsNull =
                reader.IsDBNull(4);

            Assert.Contains(
                "PurchaseRequestSubmittedDomainEvent",
                type);

            Assert.Contains(
                "purchaseRequestId",
                content,
                StringComparison.OrdinalIgnoreCase);

            Assert.True(processedOnUtcIsNull);

            Assert.Equal(
                0,
                attemptCount);

            Assert.True(lastErrorIsNull);
        }
    }
}