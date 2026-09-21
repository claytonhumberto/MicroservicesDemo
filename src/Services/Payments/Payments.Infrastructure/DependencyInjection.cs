using BuildingBlocks.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Payments.Application.Clients;
using Payments.Domain.Repositories;
using Payments.Infrastructure.Clients;
using Payments.Infrastructure.Persistence;
using Payments.Infrastructure.Repositories;

namespace Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PaymentsDb")));

        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddHttpClient<IPaymentProviderClient, FakePaymentProviderClient>(client =>
        {
            var providerUrl = configuration["PaymentProviderUrl"]
                ?? throw new InvalidOperationException("PaymentProviderUrl is not configured.");
            client.BaseAddress = new Uri(providerUrl);
            client.Timeout = TimeSpan.FromSeconds(15);
        })
        .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
        .AddStandardResilienceHandler(options =>
        {
            // Allow up to 15 s for the total attempt (including retries).
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(15);

            // Retry up to 3 times with exponential back-off — good for transient errors.
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.Delay = TimeSpan.FromMilliseconds(500);
            options.Retry.UseJitter = true;

            // Circuit breaker: open after 50% failure rate in 30 s, stay open for 10 s.
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.MinimumThroughput = 5;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(10);

            // Per-attempt timeout: give up on a single try after 5 s.
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
