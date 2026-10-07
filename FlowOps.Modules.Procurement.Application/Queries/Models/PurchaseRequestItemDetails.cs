namespace FlowOps.Modules.Procurement.Application.Queries.Models;

public sealed record PurchaseRequestItemDetails(
    Guid Id,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);