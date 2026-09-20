using BuildingBlocks.Contracts.Events;
using MassTransit;

namespace Notifications.Worker.Consumers;

public class OrderConfirmedConsumer(ILogger<OrderConfirmedConsumer> logger) : IConsumer<OrderConfirmed>
{
    public Task Consume(ConsumeContext<OrderConfirmed> context)
    {
        var evt = context.Message;

        logger.LogInformation(
            "[NOTIFICATION] Order confirmed — OrderId: {OrderId} | Customer: {CustomerName} <{CustomerEmail}> | Total: {TotalAmount} | ConfirmedAt: {ConfirmedAt}",
            evt.OrderId, evt.CustomerName, evt.CustomerEmail, evt.TotalAmount, evt.ConfirmedAt);

        return Task.CompletedTask;
    }
}
