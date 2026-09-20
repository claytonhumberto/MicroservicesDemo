namespace BuildingBlocks.Contracts.Events;

public record OrderConfirmed(
    Guid OrderId,
    string CustomerName,
    string CustomerEmail,
    decimal TotalAmount,
    DateTime ConfirmedAt);
