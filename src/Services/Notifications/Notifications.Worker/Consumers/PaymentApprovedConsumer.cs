using BuildingBlocks.Contracts.Events;
using MassTransit;

namespace Notifications.Worker.Consumers;

public class PaymentApprovedConsumer(ILogger<PaymentApprovedConsumer> logger) : IConsumer<PaymentApproved>
{
    public Task Consume(ConsumeContext<PaymentApproved> context)
    {
        var evt = context.Message;

        logger.LogInformation(
            "[NOTIFICATION] Payment approved — OrderId: {OrderId} | Amount: {Amount} {Currency} | TransactionId: {TransactionId} | Customer: {CustomerEmail} | ApprovedAt: {ApprovedAt}",
            evt.OrderId, evt.Amount, evt.Currency, evt.TransactionId, evt.CustomerEmail, evt.ApprovedAt);

        // In a real system this would send an email / push notification.
        // The consumer is kept simple intentionally — the article covers
        // how to plug in SendGrid, Firebase, or any other provider here.

        return Task.CompletedTask;
    }
}
