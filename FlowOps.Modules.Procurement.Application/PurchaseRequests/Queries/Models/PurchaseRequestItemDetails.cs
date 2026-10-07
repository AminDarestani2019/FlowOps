namespace FlowOps.Modules.Procurement.Application.PurchaseRequests.Queries.Models;

public sealed record PurchaseRequestItemDetails(
    Guid Id,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);