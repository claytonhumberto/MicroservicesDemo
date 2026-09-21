using BuildingBlocks.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.UseJwtAuthentication();

app.MapReverseProxy();

// Token endpoint — demo only, never use in production without a real identity store
app.MapPost("/auth/token", (TokenRequest request, IConfiguration config) =>
{
    // Hardcoded demo credentials — article readers swap these with a real user store
    if (request.ClientId == "demo-client" && request.ClientSecret == "demo-secret")
    {
        var token = JwtExtensions.GenerateToken(config, request.ClientId, "Service");
        return Results.Ok(new TokenResponse(token, "Bearer", config.GetSection("Jwt")["ExpiryMinutes"] ?? "60"));
    }

    return Results.Unauthorized();
})
.WithName("GetToken")
.WithSummary("Issue a JWT for demo purposes")
.AllowAnonymous();

app.MapGet("/health", () => Results.Ok(new { Service = "Gateway", Status = "Healthy", Timestamp = DateTime.UtcNow }))
   .AllowAnonymous();

app.Run();

record TokenRequest(string ClientId, string ClientSecret);
record TokenResponse(string AccessToken, string TokenType, string ExpiresInMinutes);
