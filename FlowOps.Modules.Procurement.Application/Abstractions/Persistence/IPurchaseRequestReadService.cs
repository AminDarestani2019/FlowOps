using FlowOps.Modules.Procurement.Application.PurchaseRequests.Queries.Models;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests;

namespace FlowOps.Modules.Procurement.Application.Abstractions.Persistence;

public interface IPurchaseRequestReadService
{
    Task<PurchaseRequestDetails?> GetByIdAsync(
        PurchaseRequestId id,
        CancellationToken cancellationToken = default);

    Task<PagedResult<PurchaseRequestListItem>> GetAsync(
        string? search,
        string? status,
        string sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedResult<PurchaseRequestListItem>> GetByRequesterAsync(
        EmployeeId requesterId,
        string? search,
        string? status,
        string sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}