using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging;

public static class MassTransitExtensions
{
    /// <summary>
    /// Registers MassTransit with RabbitMQ as publisher only (no consumers).
    /// Use AddMassTransitWithConsumers for services that also consume messages.
    /// </summary>
    public static IServiceCollection AddMassTransitPublisher(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((ctx, cfg) => ConfigureRabbitMq(cfg, configuration));
        });

        return services;
    }

    /// <summary>
    /// Registers MassTransit with RabbitMQ and the provided consumers.
    /// </summary>
    public static IServiceCollection AddMassTransitWithConsumers(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator> configureConsumers)
    {
        services.AddMassTransit(x =>
        {
            configureConsumers(x);
            x.UsingRabbitMq((ctx, cfg) =>
            {
                ConfigureRabbitMq(cfg, configuration);
                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }

    private static void ConfigureRabbitMq(IRabbitMqBusFactoryConfigurator cfg, IConfiguration configuration)
    {
        var host = configuration["RabbitMQ:Host"] ?? "localhost";
        var username = configuration["RabbitMQ:Username"] ?? "guest";
        var password = configuration["RabbitMQ:Password"] ?? "guest";

        cfg.Host(host, "/", h =>
        {
            h.Username(username);
            h.Password(password);
        });
    }
}
