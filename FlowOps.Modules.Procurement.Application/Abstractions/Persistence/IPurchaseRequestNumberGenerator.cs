using FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;

namespace FlowOps.Modules.Procurement.Application.Abstractions.Persistence;

public interface IPurchaseRequestNumberGenerator
{
    Task<PurchaseRequestNumber> GenerateAsync(
        CancellationToken cancellationToken = default);
}
