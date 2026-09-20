using Catalog.Application.Services;
using Catalog.Infrastructure;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCatalogInfrastructure(builder.Configuration);
builder.Services.AddScoped<ProductService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await db.Database.MigrateAsync();
    await CatalogSeeder.SeedAsync(db);
}

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Catalog Service API";
    options.Theme = ScalarTheme.Purple;
});

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { Service = "Catalog", Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.Run();
