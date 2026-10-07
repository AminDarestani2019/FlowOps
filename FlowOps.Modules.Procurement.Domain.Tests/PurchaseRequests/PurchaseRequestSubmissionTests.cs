using FlowOps.Modules.Procurement.Domain.PurchaseRequests;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.Events;

namespace FlowOps.Modules.Procurement.Domain.Tests.PurchaseRequests;

public sealed class PurchaseRequestSubmissionTests
{

    [Fact]
    public void Submit_WithItem_ShouldRaiseSubmittedDomainEvent()
    {
        var request =
            PurchaseRequestTestData.CreateDraft();

        var money =
            PurchaseRequestTestData.CreateMoney(
                1500m);

        var addResult =
            request.AddItem(
                "Development Laptop",
                1,
                money);

        Assert.True(addResult.IsSuccess);

        var submittedAt =
            new DateTime(
                2026,
                10,
                7,
                12,
                0,
                0,
                DateTimeKind.Utc);

        var result =
            request.Submit(
                submittedAt);

        Assert.True(result.IsSuccess);

        Assert.Equal(
            PurchaseRequestStatus.Submitted,
            request.Status);

        Assert.Equal(
            submittedAt,
            request.SubmittedAtUtc);

        var domainEvent =
            Assert.Single(
                request.DomainEvents
                    .OfType<
                        PurchaseRequestSubmittedDomainEvent>());

        Assert.Equal(
            request.Id,
            domainEvent.PurchaseRequestId);

        Assert.Equal(
            submittedAt,
            domainEvent.OccurredOnUtc);
    }

    [Fact]
    public void Submit_WhenAlreadySubmitted_ShouldFail()
    {
        var request =
            PurchaseRequestTestData.CreateDraft();

        var money =
            PurchaseRequestTestData.CreateMoney(1000m);

        request.AddItem(
            "Laptop",
            1,
            money);

        var firstSubmit = request.Submit(
            new DateTime(
                2026,
                10,
                5,
                10,
                0,
                0,
                DateTimeKind.Utc));

        Assert.True(firstSubmit.IsSuccess);

        var secondSubmit = request.Submit(
            new DateTime(
                2026,
                10,
                5,
                11,
                0,
                0,
                DateTimeKind.Utc));

        Assert.True(secondSubmit.IsFailure);

        Assert.Equal(
            "PurchaseRequest.InvalidStateTransition",
            secondSubmit.Error.Code);
    }

    [Fact]
    public void AddItem_AfterSubmission_ShouldFail()
    {
        var request =
            PurchaseRequestTestData.CreateDraft();

        var money =
            PurchaseRequestTestData.CreateMoney(1000m);

        var addResult = request.AddItem(
            "Laptop",
            1,
            money);

        Assert.True(addResult.IsSuccess);

        var submitResult = request.Submit(
            new DateTime(
                2026,
                10,
                5,
                10,
                0,
                0,
                DateTimeKind.Utc));

        Assert.True(submitResult.IsSuccess);

        var result = request.AddItem(
            "Monitor",
            1,
            money);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "PurchaseRequest.CannotModifyNonDraft",
            result.Error.Code);

        Assert.Single(
            request.Items);
    }

    [Fact]
    public void Submit_WithItem_ShouldChangeStatusToSubmitted()
    {
        var request =
            PurchaseRequestTestData.CreateDraft();

        var money =
            PurchaseRequestTestData.CreateMoney(2000m);

        var addResult = request.AddItem(
            "Development Laptop",
            1,
            money);

        Assert.True(addResult.IsSuccess);

        var submittedAt =
            new DateTime(
                2026,
                10,
                5,
                9,
                30,
                0,
                DateTimeKind.Utc);

        var result =
            request.Submit(submittedAt);

        Assert.True(result.IsSuccess);

        Assert.Equal(
            PurchaseRequestStatus.Submitted,
            request.Status);

        Assert.Equal(
            submittedAt,
            request.SubmittedAtUtc);
    }

    [Fact]
    public void Submit_WithoutItems_ShouldFail()
    {
        var request =
            PurchaseRequestTestData.CreateDraft();

        var submittedAt =
            new DateTime(
                2026,
                10,
                5,
                9,
                0,
                0,
                DateTimeKind.Utc);

        var result =
            request.Submit(submittedAt);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "PurchaseRequest.EmptyRequest",
            result.Error.Code);

        Assert.Equal(
            PurchaseRequestStatus.Draft,
            request.Status);

        Assert.Null(
            request.SubmittedAtUtc);
    }
}