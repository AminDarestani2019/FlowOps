using FlowOps.BuildingBlocks.Domain.Time;
using FlowOps.Modules.Procurement.Application.Abstractions.Persistence;
using FlowOps.Modules.Procurement.Infrastructure.Persistence;
using FlowOps.Modules.Procurement.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlowOps.Modules.Procurement.Infrastructure.DependencyInjection;

public static class ProcurementInfrastructureExtensions
{
    public static IServiceCollection AddProcurementInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        // PostgreSQL / EF Core
        services.AddDbContext<ProcurementDbContext>(
            options =>
            {
                options.UseNpgsql(connectionString);
            });

        // Unit of Work
        services.AddScoped<
            IProcurementUnitOfWork,
            ProcurementUnitOfWork>();

        // Request Number Generator
        services.AddScoped<
            IPurchaseRequestNumberGenerator,
            PostgreSqlPurchaseRequestNumberGenerator>();

        services.AddScoped<
            IPurchaseRequestRepository,
            PurchaseRequestRepository>();
        // Clock
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
