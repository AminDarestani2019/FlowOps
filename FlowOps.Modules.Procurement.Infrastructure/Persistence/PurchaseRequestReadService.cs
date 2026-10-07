using FlowOps.Modules.Procurement.Application.Abstractions.Persistence;
using FlowOps.Modules.Procurement.Application.PurchaseRequests.Queries.Models;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests;
using FlowOps.Modules.Procurement.Domain.PurchaseRequests.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FlowOps.Modules.Procurement.Infrastructure.Persistence;

public sealed class PurchaseRequestReadService
    : IPurchaseRequestReadService
{
    private readonly ProcurementDbContext _dbContext;

    public PurchaseRequestReadService(
        ProcurementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PurchaseRequestDetails?> GetByIdAsync(
        PurchaseRequestId id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PurchaseRequests
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x =>
                new PurchaseRequestDetails(
                    x.Id.Value,
                    x.RequestNumber.Value,
                    x.RequesterId.Value,
                    x.DepartmentId.Value,
                    x.Currency.Code,
                    x.Status.ToString(),
                    x.CreatedAtUtc,
                    x.SubmittedAtUtc,

                    x.Items.Sum(
                        item =>
                            item.Quantity *
                            item.UnitPrice.Amount),

                    x.Items
                        .Select(item =>
                            new PurchaseRequestItemDetails(
                                item.Id.Value,
                                item.Description,
                                item.Quantity,
                                item.UnitPrice.Amount,
                                item.Quantity *
                                item.UnitPrice.Amount))
                        .ToList()
                ))
            .SingleOrDefaultAsync(
                cancellationToken);
    }


    public async Task<PagedResult<PurchaseRequestListItem>> GetAsync(
        string? search,
        string? status,
        string sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query =
       _dbContext.PurchaseRequests
           .AsNoTracking()
           .AsQueryable();

        // -----------------------------------------------------
        // Search
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm =
                search.Trim();

            var searchPattern =
                $"%{searchTerm}%";

            var requestNumberResult =
                PurchaseRequestNumber.Create(
                    searchTerm);

            if (requestNumberResult.IsSuccess)
            {
                var requestNumber =
                    requestNumberResult.Value;

                query =
                    query.Where(x =>
                        x.RequestNumber == requestNumber ||
                        x.Items.Any(item =>
                            EF.Functions.ILike(
                                item.Description,
                                searchPattern)));
            }
            else
            {
                query =
                    query.Where(x =>
                        x.Items.Any(item =>
                            EF.Functions.ILike(
                                item.Description,
                                searchPattern)));
            }
        }

        // -----------------------------------------------------
        // Status Filter
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (
                Enum.TryParse<PurchaseRequestStatus>(
                    status,
                    ignoreCase: true,
                    out var parsedStatus)
            )
            {
                query =
                    query.Where(
                        x => x.Status == parsedStatus);
            }
            else
            {
                // Unknown status means no valid rows.
                query =
                    query.Where(_ => false);
            }
        }

        // -----------------------------------------------------
        // Total Count
        //
        // IMPORTANT:
        // Count before Skip / Take.
        // -----------------------------------------------------

        var totalCount =
            await query.CountAsync(
                cancellationToken);

        // -----------------------------------------------------
        // Sorting
        // -----------------------------------------------------

        var normalizedSortBy =
            sortBy
                .Trim()
                .ToLowerInvariant();

        query =
            normalizedSortBy switch
            {
                "total" when descending =>
                    query.OrderByDescending(
                        x =>
                            x.Items
                                .Select(item =>
                                    (decimal?)(
                                        item.Quantity *
                                        item.UnitPrice.Amount))
                                .Sum() ?? 0m),

                "total" =>
                    query.OrderBy(
                        x =>
                            x.Items
                                .Select(item =>
                                    (decimal?)(
                                        item.Quantity *
                                        item.UnitPrice.Amount))
                                .Sum() ?? 0m),

                "itemcount" when descending =>
                    query.OrderByDescending(
                        x => x.Items.Count),

                "itemcount" =>
                    query.OrderBy(
                        x => x.Items.Count),

                "status" when descending =>
                    query.OrderByDescending(
                        x => x.Status),

                "status" =>
                    query.OrderBy(
                        x => x.Status),

                _ when descending =>
                    query.OrderByDescending(
                        x => x.CreatedAtUtc),

                _ =>
                    query.OrderBy(
                        x => x.CreatedAtUtc)
            };

        // -----------------------------------------------------
        // Pagination + Projection
        // -----------------------------------------------------

        var rows =
            await query
                .Skip(
                    (page - 1) *
                    pageSize)
                .Take(pageSize)
                .Select(x =>
                    new
                    {
                        x.Id,
                        x.RequestNumber,
                        x.Status,
                        x.Currency,
                        x.CreatedAtUtc,

                        ItemCount =
                            x.Items.Count,

                        TotalAmount =
                            x.Items
                                .Select(item =>
                                    (decimal?)(
                                        item.Quantity *
                                        item.UnitPrice.Amount))
                                .Sum()
                            ?? 0m
                    })
                .ToListAsync(
                    cancellationToken);

        // -----------------------------------------------------
        // Read Model Mapping
        // -----------------------------------------------------

        var items =
            rows
                .Select(row =>
                    new PurchaseRequestListItem(
                        row.Id.Value,
                        row.RequestNumber.Value,
                        row.Status.ToString(),
                        row.Currency.Code,
                        row.TotalAmount,
                        row.ItemCount,
                        row.CreatedAtUtc))
                .ToList();

        return new PagedResult<PurchaseRequestListItem>(
            items,
            page,
            pageSize,
            totalCount);
    }


    public async Task<PagedResult<PurchaseRequestListItem>> GetByRequesterAsync(
        EmployeeId requesterId,
        string? search,
        string? status,
        string sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query =
       _dbContext.PurchaseRequests
           .AsNoTracking()
           .Where(x =>
               x.RequesterId == requesterId)
           .AsQueryable();


        // -----------------------------------------------------
        // Search
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm =
                search.Trim();

            var searchPattern =
                $"%{searchTerm}%";

            var requestNumberResult =
                PurchaseRequestNumber.Create(
                    searchTerm);

            if (requestNumberResult.IsSuccess)
            {
                var requestNumber =
                    requestNumberResult.Value;

                query =
                    query.Where(x =>
                        x.RequestNumber == requestNumber ||
                        x.Items.Any(item =>
                            EF.Functions.ILike(
                                item.Description,
                                searchPattern)));
            }
            else
            {
                query =
                    query.Where(x =>
                        x.Items.Any(item =>
                            EF.Functions.ILike(
                                item.Description,
                                searchPattern)));
            }
        }


        // -----------------------------------------------------
        // Status Filter
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (
                Enum.TryParse<PurchaseRequestStatus>(
                    status,
                    ignoreCase: true,
                    out var parsedStatus)
            )
            {
                query =
                    query.Where(
                        x => x.Status == parsedStatus);
            }
            else
            {
                query =
                    query.Where(_ => false);
            }
        }


        // -----------------------------------------------------
        // Total Count
        // -----------------------------------------------------

        var totalCount =
            await query.CountAsync(
                cancellationToken);


        // -----------------------------------------------------
        // Sorting
        // -----------------------------------------------------

        var normalizedSortBy =
            sortBy
                .Trim()
                .ToLowerInvariant();

        query =
            normalizedSortBy switch
            {
                "total" when descending =>
                    query.OrderByDescending(
                        x =>
                            x.Items
                                .Select(item =>
                                    (decimal?)(
                                        item.Quantity *
                                        item.UnitPrice.Amount))
                                .Sum() ?? 0m),

                "total" =>
                    query.OrderBy(
                        x =>
                            x.Items
                                .Select(item =>
                                    (decimal?)(
                                        item.Quantity *
                                        item.UnitPrice.Amount))
                                .Sum() ?? 0m),

                "itemcount" when descending =>
                    query.OrderByDescending(
                        x => x.Items.Count),

                "itemcount" =>
                    query.OrderBy(
                        x => x.Items.Count),

                "status" when descending =>
                    query.OrderByDescending(
                        x => x.Status),

                "status" =>
                    query.OrderBy(
                        x => x.Status),

                _ when descending =>
                    query.OrderByDescending(
                        x => x.CreatedAtUtc),

                _ =>
                    query.OrderBy(
                        x => x.CreatedAtUtc)
            };


        // -----------------------------------------------------
        // Pagination + Projection
        // -----------------------------------------------------

        var rows =
            await query
                .Skip(
                    (page - 1) *
                    pageSize)
                .Take(pageSize)
                .Select(x =>
                    new
                    {
                        x.Id,
                        x.RequestNumber,
                        x.Status,
                        x.Currency,
                        x.CreatedAtUtc,

                        ItemCount =
                            x.Items.Count,

                        TotalAmount =
                            x.Items
                                .Select(item =>
                                    (decimal?)(
                                        item.Quantity *
                                        item.UnitPrice.Amount))
                                .Sum()
                            ?? 0m
                    })
                .ToListAsync(
                    cancellationToken);


        // -----------------------------------------------------
        // Read Model Mapping
        // -----------------------------------------------------

        var items =
            rows
                .Select(row =>
                    new PurchaseRequestListItem(
                        row.Id.Value,
                        row.RequestNumber.Value,
                        row.Status.ToString(),
                        row.Currency.Code,
                        row.TotalAmount,
                        row.ItemCount,
                        row.CreatedAtUtc))
                .ToList();


        return new PagedResult<PurchaseRequestListItem>(
            items,
            page,
            pageSize,
            totalCount);
    }
}
