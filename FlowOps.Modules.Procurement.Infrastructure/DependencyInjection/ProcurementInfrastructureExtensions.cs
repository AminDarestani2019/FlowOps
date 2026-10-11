using FlowOps.BuildingBlocks.Domain.Time;
using FlowOps.Modules.Procurement.Application.Abstractions.Persistence;
using FlowOps.Modules.Procurement.Application.Abstractions.Workflow;
using FlowOps.Modules.Procurement.Infrastructure.Persistence;
using FlowOps.Modules.Procurement.Infrastructure.Persistence.Outbox;
using FlowOps.Modules.Procurement.Infrastructure.Time;
using FlowOps.Modules.Procurement.Infrastructure.Workflow.Camunda;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FlowOps.Modules.Procurement.Infrastructure.DependencyInjection;

public static class ProcurementInfrastructureExtensions
{
    public static IServiceCollection AddProcurementInfrastructure(
        this IServiceCollection services,
        string connectionString,
        IConfiguration configuration)
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

        services.AddScoped<
            IPurchaseRequestReadService,
            PurchaseRequestReadService>();

        // Clock
        services.AddSingleton<IClock, SystemClock>();

        // -----------------------------------------------------
        // Camunda
        // -----------------------------------------------------

        services.Configure<CamundaOptions>(
            configuration.GetSection(
                CamundaOptions.SectionName));

        services.AddHttpClient<
            IWorkflowProcessReader,
            CamundaWorkflowProcessReader>(
            (serviceProvider, client) =>
            {
                var options =
                    serviceProvider
                        .GetRequiredService<IOptions<CamundaOptions>>()
                        .Value;

                client.BaseAddress =
                    new Uri(options.BaseUrl);
            });


        services.AddHttpClient<
            IWorkflowVariableReader,
            CamundaWorkflowVariableReader>(
            (serviceProvider, client) =>
            {
                var options =
                    serviceProvider
                        .GetRequiredService<IOptions<CamundaOptions>>()
                        .Value;

                client.BaseAddress =
                    new Uri(options.BaseUrl);
            });
            
        services.AddHttpClient<
            IPurchaseRequestWorkflowStarter,
            CamundaPurchaseRequestWorkflowStarter>(
            (serviceProvider, client) =>
            {
                var options = serviceProvider
                    .GetRequiredService<IOptions<CamundaOptions>>()
                    .Value;

                client.BaseAddress = new Uri(options.BaseUrl);
            });

        services.AddHttpClient<
            IWorkflowTaskReader,
            CamundaWorkflowTaskReader>(
            (serviceProvider, client) =>
            {
                var options = serviceProvider
                    .GetRequiredService<IOptions<CamundaOptions>>()
                    .Value;

                client.BaseAddress = new Uri(options.BaseUrl);
            });

        services.AddHttpClient<
            IWorkflowTaskCompleter,
            CamundaWorkflowTaskCompleter>(
            (serviceProvider, client) =>
            {
                var options =
                    serviceProvider
                        .GetRequiredService<IOptions<CamundaOptions>>()
                        .Value;

                client.BaseAddress =
                    new Uri(options.BaseUrl);
            });

        services.Configure<OutboxProcessorOptions>(configuration.GetSection(OutboxProcessorOptions.SectionName));

        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
