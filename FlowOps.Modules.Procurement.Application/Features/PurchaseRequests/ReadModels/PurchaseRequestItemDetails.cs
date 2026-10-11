namespace FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.Models;

public sealed record PurchaseRequestItemDetails(
    Guid Id,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);