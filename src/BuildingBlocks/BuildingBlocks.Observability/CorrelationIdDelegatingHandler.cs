using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Observability;

/// <summary>
/// Propagates the X-Correlation-ID header from the current HTTP request context
/// to all outgoing HttpClient calls, so the correlation ID flows across service boundaries.
/// </summary>
public class CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.HeaderName] as string;

        if (!string.IsNullOrEmpty(correlationId))
            request.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, correlationId);

        return base.SendAsync(request, cancellationToken);
    }
}
