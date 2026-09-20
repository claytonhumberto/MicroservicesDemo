namespace BuildingBlocks.Contracts.Events;

public record PaymentApproved(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string TransactionId,
    string CustomerEmail,
    DateTime ApprovedAt);
