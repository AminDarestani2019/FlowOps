using FlowOps.Api.Development;
using FlowOps.Api.Endpoints.Procurement;
using FlowOps.Modules.Procurement.Application.Abstractions.Identity;
using FlowOps.Modules.Procurement.Application.PurchaseRequests.CreatePurchaseRequest;
using FlowOps.Modules.Procurement.Application.PurchaseRequests.AddPurchaseRequestItem;
using FlowOps.Modules.Procurement.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var procurementConnectionString =
    builder.Configuration.GetConnectionString(
        "ProcurementDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'ProcurementDatabase' was not found.");

builder.Services.AddProcurementInfrastructure(
    procurementConnectionString);

builder.Services.AddScoped<CreatePurchaseRequestHandler>();
builder.Services.AddScoped<AddPurchaseRequestItemHandler>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// --------------------------------------------
// Development Identity
// --------------------------------------------

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<
        ICurrentUser,
        DevelopmentCurrentUser>();
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

app.Run();