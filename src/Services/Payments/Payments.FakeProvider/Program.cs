var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Configurable behavior via query string: ?behavior=success|fail|timeout|slow
app.MapPost("/charge", async (ChargeRequest request, HttpContext ctx) =>
{
    var behavior = ctx.Request.Query["behavior"].FirstOrDefault() ?? "success";

    await Task.Delay(behavior switch
    {
        "slow" => 3000,
        "timeout" => 31000,
        _ => 200
    });

    return behavior switch
    {
        "fail" => Results.UnprocessableEntity(new { Error = "Payment declined by provider" }),
        "timeout" => Results.StatusCode(504),
        _ => Results.Ok(new
        {
            TransactionId = Guid.NewGuid(),
            Status = "Approved",
            Amount = request.Amount,
            ProcessedAt = DateTime.UtcNow
        })
    };
});

app.MapGet("/health", () => Results.Ok(new { Service = "FakePaymentProvider", Status = "Healthy" }));

app.Run();

record ChargeRequest(Guid OrderId, decimal Amount, string Currency, string CardToken);
