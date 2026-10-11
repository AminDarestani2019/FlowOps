using FlowOps.BuildingBlocks.Domain.Events;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.Events;

namespace FlowOps.Modules.Procurement.Domain.Tests.PurchaseRequests;

public sealed class PurchaseRequestApprovalTests
{
    [Fact]
    public void ApproveStep_WhenNotInApproval_ShouldFail()
    {
        var request =
            PurchaseRequestTestData.CreateSubmitted();

        var result =
            request.ApproveStep(
                PurchaseRequestApprovalStage.Manager,
                false,
                DateTime.UtcNow);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "PurchaseRequest.InvalidStateTransition",
            result.Error.Code);

        Assert.Equal(
            PurchaseRequestStatus.Submitted,
            request.Status);
    }

    [Fact]
    public void ApproveStep_WhenFinal_ShouldChangeStatusToApproved()
    {
        var request =
            PurchaseRequestTestData.CreateInApproval();

        var approvedAtUtc =
            new DateTime(
                2026,
                10,
                11,
                7,
                30,
                0,
                DateTimeKind.Utc);

        var result =
            request.ApproveStep(
                PurchaseRequestApprovalStage.Procurement,
                true,
                approvedAtUtc);

        Assert.True(result.IsSuccess);

        Assert.Equal(
            PurchaseRequestStatus.Approved,
            request.Status);

        var stepEvent =
            Assert.Single(
                request.DomainEvents
                    .OfType<
                        PurchaseRequestApprovalStepApprovedDomainEvent>());

        Assert.Equal(
            PurchaseRequestApprovalStage.Procurement,
            stepEvent.Stage);

        Assert.True(
            stepEvent.IsFinalApproval);

        var approvedEvent =
            Assert.Single(
                request.DomainEvents
                    .OfType<
                        PurchaseRequestApprovedDomainEvent>());

        Assert.Equal(
            request.Id,
            approvedEvent.PurchaseRequestId);
    }

    [Fact]
    public void ApproveStep_WhenIntermediate_ShouldRemainInApproval()
    {
        var request = PurchaseRequestTestData.CreateInApproval();

        var approvedAtUtc =
            new DateTime(
                2026,
                10,
                11,
                7,
                0,
                0,
                DateTimeKind.Utc);

        var result =
            request.ApproveStep(
                PurchaseRequestApprovalStage.Manager,
                false,
                approvedAtUtc);

        Assert.True(result.IsSuccess);

        Assert.Equal(
            PurchaseRequestStatus.InApproval,
            request.Status);

        var domainEvent =
            Assert.Single(
                request.DomainEvents
                    .OfType<
                        PurchaseRequestApprovalStepApprovedDomainEvent>());

        Assert.Equal(
            PurchaseRequestApprovalStage.Manager,
            domainEvent.Stage);

        Assert.False(
            domainEvent.IsFinalApproval);
    }

    [Fact]
    public void MarkInApproval_WhenSubmitted_ShouldChangeStatusToInApproval()
    {
        var request =
            PurchaseRequestTestData.CreateSubmitted();

        var result =
            request.MarkInApproval();

        Assert.True(result.IsSuccess);

        Assert.Equal(
            PurchaseRequestStatus.InApproval,
            request.Status);
    }

    [Fact]
    public void MarkInApproval_WhenDraft_ShouldFail()
    {
        var request =
            PurchaseRequestTestData.CreateDraft();

        var result =
            request.MarkInApproval();

        Assert.True(result.IsFailure);

        Assert.Equal(
            "PurchaseRequest.InvalidStateTransition",
            result.Error.Code);

        Assert.Equal(
            PurchaseRequestStatus.Draft,
            request.Status);
    }

    [Fact]
    public void Approve_WhenInApproval_ShouldChangeStatusToApproved()
    {
        var request =
            PurchaseRequestTestData.CreateInApproval();

        var approvedAtUtc =
            new DateTime(
                2026,
                10,
                10,
                9,
                0,
                0,
                DateTimeKind.Utc);

        var result =
            request.Approve(
                approvedAtUtc);

        Assert.True(result.IsSuccess);

        Assert.Equal(
            PurchaseRequestStatus.Approved,
            request.Status);
    }

    [Fact]
    public void Approve_WhenInApproval_ShouldRaiseApprovedDomainEvent()
    {
        var request =
            PurchaseRequestTestData.CreateInApproval();

        var approvedAtUtc =
            new DateTime(
                2026,
                10,
                10,
                9,
                0,
                0,
                DateTimeKind.Utc);

        var result =
            request.Approve(
                approvedAtUtc);

        Assert.True(result.IsSuccess);

        var domainEvent =
            Assert.Single(
                request.DomainEvents
                    .OfType<
                        PurchaseRequestApprovedDomainEvent>());

        Assert.Equal(
            request.Id,
            domainEvent.PurchaseRequestId);

        Assert.Equal(
            approvedAtUtc,
            domainEvent.OccurredOnUtc);
    }

    [Fact]
    public void Approve_WhenSubmitted_ShouldFail()
    {
        var request =
            PurchaseRequestTestData.CreateSubmitted();

        var result =
            request.Approve(
                new DateTime(
                    2026,
                    10,
                    10,
                    9,
                    0,
                    0,
                    DateTimeKind.Utc));

        Assert.True(result.IsFailure);

        Assert.Equal(
            "PurchaseRequest.InvalidStateTransition",
            result.Error.Code);

        Assert.Equal(
            PurchaseRequestStatus.Submitted,
            request.Status);
    }

    [Fact]
    public void Reject_WhenInApproval_ShouldChangeStatusToRejected()
    {
        var request =
            PurchaseRequestTestData.CreateInApproval();

        var rejectedAtUtc =
            new DateTime(
                2026,
                10,
                10,
                9,
                30,
                0,
                DateTimeKind.Utc);

        var result =
            request.Reject(
                rejectedAtUtc);

        Assert.True(result.IsSuccess);

        Assert.Equal(
            PurchaseRequestStatus.Rejected,
            request.Status);
    }

    [Fact]
    public void Reject_WhenInApproval_ShouldRaiseRejectedDomainEvent()
    {
        var request =
            PurchaseRequestTestData.CreateInApproval();

        var rejectedAtUtc =
            new DateTime(
                2026,
                10,
                10,
                9,
                30,
                0,
                DateTimeKind.Utc);

        var result =
            request.Reject(
                rejectedAtUtc);

        Assert.True(result.IsSuccess);

        var domainEvent =
            Assert.Single(
                request.DomainEvents
                    .OfType<
                        PurchaseRequestRejectedDomainEvent>());

        Assert.Equal(
            request.Id,
            domainEvent.PurchaseRequestId);

        Assert.Equal(
            rejectedAtUtc,
            domainEvent.OccurredOnUtc);
    }

    [Fact]
    public void Reject_WhenSubmitted_ShouldFail()
    {
        var request =
            PurchaseRequestTestData.CreateSubmitted();

        var result =
            request.Reject(
                new DateTime(
                    2026,
                    10,
                    10,
                    9,
                    30,
                    0,
                    DateTimeKind.Utc));

        Assert.True(result.IsFailure);

        Assert.Equal(
            "PurchaseRequest.InvalidStateTransition",
            result.Error.Code);

        Assert.Equal(
            PurchaseRequestStatus.Submitted,
            request.Status);
    }

    [Fact]
    public void Approve_WhenRejected_ShouldFail()
    {
        var request =
            PurchaseRequestTestData.CreateInApproval();

        var rejectResult =
            request.Reject(
                new DateTime(
                    2026,
                    10,
                    10,
                    9,
                    30,
                    0,
                    DateTimeKind.Utc));

        Assert.True(rejectResult.IsSuccess);

        var approveResult =
            request.Approve(
                new DateTime(
                    2026,
                    10,
                    10,
                    10,
                    0,
                    0,
                    DateTimeKind.Utc));

        Assert.True(approveResult.IsFailure);

        Assert.Equal(
            "PurchaseRequest.InvalidStateTransition",
            approveResult.Error.Code);

        Assert.Equal(
            PurchaseRequestStatus.Rejected,
            request.Status);
    }
}