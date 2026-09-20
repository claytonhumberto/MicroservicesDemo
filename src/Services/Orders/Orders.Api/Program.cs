using Microsoft.EntityFrameworkCore;
using Orders.Application.Services;
using Orders.Infrastructure;
using Orders.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddOrdersInfrastructure(builder.Configuration);
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

// Run migrations and ensure database exists on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    await db.Database.MigrateAsync();
}

app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { Service = "Orders", Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.Run();
