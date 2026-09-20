using BuildingBlocks.Messaging;
using Notifications.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransitWithConsumers(builder.Configuration, x =>
{
    x.AddConsumer<PaymentApprovedConsumer>();
    x.AddConsumer<OrderConfirmedConsumer>();
});

var host = builder.Build();
host.Run();
