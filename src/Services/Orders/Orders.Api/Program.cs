using BuildingBlocks.Messaging;
using BuildingBlocks.Observability;
using Microsoft.EntityFrameworkCore;
using Orders.Application.Services;
using Orders.Infrastructure;
using Orders.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability("orders-service");
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddOrdersInfrastructure(builder.Configuration);
builder.Services.AddMassTransitPublisher(builder.Configuration);
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCorrelationId();
app.UseJwtAuthentication();
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Orders Service API";
    options.Theme = ScalarTheme.Solarized;
});
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { Service = "Orders", Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.Run();
