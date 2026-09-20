using BuildingBlocks.Messaging;
using Microsoft.EntityFrameworkCore;
using Payments.Application.Services;
using Payments.Infrastructure;
using Payments.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddPaymentsInfrastructure(builder.Configuration);
builder.Services.AddMassTransitPublisher(builder.Configuration);
builder.Services.AddScoped<PaymentService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    await db.Database.MigrateAsync();
}

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Payments Service API";
    options.Theme = ScalarTheme.Mars;
});
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { Service = "Payments", Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.Run();
