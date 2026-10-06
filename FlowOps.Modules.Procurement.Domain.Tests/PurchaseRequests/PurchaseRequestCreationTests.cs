using FlowOps.Modules.Procurement.Domain.PurchaseRequests;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.Events;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;

namespace FlowOps.Modules.Procurement.Domain.Tests.PurchaseRequests;

public sealed class PurchaseRequestCreationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateDraftRequest()
    {
        var numberResult =
            PurchaseRequestNumber.Create("PR-2026-000001");

        var currencyResult =
            Currency.Create("EUR");

        Assert.True(numberResult.IsSuccess);
        Assert.True(currencyResult.IsSuccess);

        var requesterId = EmployeeId.New();
        var departmentId = DepartmentId.New();

        var createdAt =
            new DateTime(
                2026,
                10,
                5,
                8,
                0,
                0,
                DateTimeKind.Utc);

        var result = PurchaseRequest.Create(
            numberResult.Value,
            requesterId,
            departmentId,
            currencyResult.Value,
            createdAt);

        Assert.True(result.IsSuccess);

        var request = result.Value;

        Assert.Equal(
            PurchaseRequestStatus.Draft,
            request.Status);

        Assert.Equal(
            requesterId,
            request.RequesterId);

        Assert.Equal(
            departmentId,
            request.DepartmentId);

        Assert.Equal(
            createdAt,
            request.CreatedAtUtc);

        Assert.Null(
            request.SubmittedAtUtc);

        Assert.Empty(
            request.Items);

        Assert.Equal(
            0m,
            request.TotalAmount);
    }

    [Fact]
    public void Create_WithValidData_ShouldRaiseCreatedDomainEvent()
    {
        var request =
            PurchaseRequestTestData.CreateDraft();

        var domainEvent =
            Assert.Single(request.DomainEvents);

        var createdEvent =
            Assert.IsType<PurchaseRequestCreatedDomainEvent>(
                domainEvent);

        Assert.Equal(
            request.Id,
            createdEvent.PurchaseRequestId);

        Assert.Equal(
            request.RequesterId,
            createdEvent.RequesterId);

        Assert.Equal(
            request.DepartmentId,
            createdEvent.DepartmentId);
    }
}