var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { Service = "Payments", Status = "Healthy", Timestamp = DateTime.UtcNow }));
app.MapGet("/", () => Results.Ok(new { Service = "Payments Service", Version = "1.0", Note = "Phase 4 — coming soon" }));

app.Run();
