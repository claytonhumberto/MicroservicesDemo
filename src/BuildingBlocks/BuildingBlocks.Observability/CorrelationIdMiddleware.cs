using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace BuildingBlocks.Observability;

/// <summary>
/// Reads X-Correlation-ID from the incoming request (or generates one), adds it to
/// the response, and pushes it into Serilog's LogContext so every log line in this
/// request includes the correlation ID automatically.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
