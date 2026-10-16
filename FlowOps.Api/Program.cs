using FlowOps.Api.Development;
using FlowOps.Api.Endpoints.Procurement;
using FlowOps.Api.Endpoints.Workflow;
using FlowOps.Modules.Procurement.Application.Abstractions.Identity;
using FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.AddPurchaseRequestItem;
using FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.ApprovePurchaseRequest;
using FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.CreatePurchaseRequest;
using FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.GetMyPurchaseRequests;
using FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.GetPurchaseRequest;
using FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.GetPurchaseRequests;
using FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.SubmitPurchaseRequest;
using FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.RejectPurchaseRequest;
using FlowOps.Modules.Procurement.Infrastructure.DependencyInjection;
using FlowOps.Modules.Procurement.Application.Features.PurchaseRequests.AcknowledgePurchaseRequest;

var builder = WebApplication.CreateBuilder(args);

var procurementConnectionString =
    builder.Configuration.GetConnectionString(
        "ProcurementDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'ProcurementDatabase' was not found.");

builder.Services.AddProcurementInfrastructure(procurementConnectionString, builder.Configuration);

builder.Services.AddScoped<CreatePurchaseRequestHandler>();
builder.Services.AddScoped<AddPurchaseRequestItemHandler>();
builder.Services.AddScoped<GetPurchaseRequestHandler>();
builder.Services.AddScoped<GetPurchaseRequestsHandler>();
builder.Services.AddScoped<GetMyPurchaseRequestsHandler>();
builder.Services.AddScoped<SubmitPurchaseRequestHandler>();
builder.Services.AddScoped<ApprovePurchaseRequestHandler>();
builder.Services.AddScoped<RejectPurchaseRequestHandler>();
builder.Services.AddScoped<AcknowledgePurchaseRequestHandler>();
builder.Services.AddScoped<FlowOps.Modules.Procurement.Application.Features.Workflow.GetMyTasks.Handler>();
builder.Services.AddScoped<FlowOps.Modules.Procurement.Application.Features.Workflow.GetTaskDetails.Handler>();
builder.Services.AddScoped<FlowOps.Modules.Procurement.Application.Features.Workflow.CompleteTask.Handler>();

builder.Services.AddScoped<FlowOps.Modules.Procurement.Application.Features.Workflow.ClaimTask.Handler>();
builder.Services.AddScoped<FlowOps.Modules.Procurement.Application.Features.Workflow.UnclaimTask.Handler>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// --------------------------------------------
// Development Identity
// --------------------------------------------

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<
        ICurrentUser,
        DevelopmentCurrentUser>();

    builder.Services.AddScoped<
        IWorkflowCurrentUser,
        DevelopmentWorkflowCurrentUser>();
}
else
{
    throw new InvalidOperationException(
        "Production authentication is not configured yet.");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        application = "FlowOps",
        status = "Running"
    });
});

// --------------------------------------------
// Procurement Endpoints
// --------------------------------------------

CreatePurchaseRequestEndpoint.Map(app);
AddPurchaseRequestItemEndpoint.Map(app);
GetPurchaseRequestEndpoint.Map(app);
GetPurchaseRequestsEndpoint.Map(app);
GetMyPurchaseRequestsEndpoint.Map(app);
SubmitPurchaseRequestEndpoint.Map(app);
GetMyTasksEndpoint.Map(app);
GetTaskDetailsEndpoint.Map(app);
CompleteTaskEndpoint.Map(app);

ClaimTaskEndpoint.Map(app);
UnclaimTaskEndpoint.Map(app);
ApprovePurchaseRequestEndpoint.Map(app);
RejectPurchaseRequestEndpoint.Map(app);
AcknowledgePurchaseRequestEndpoint.Map(app);

app.Run();
