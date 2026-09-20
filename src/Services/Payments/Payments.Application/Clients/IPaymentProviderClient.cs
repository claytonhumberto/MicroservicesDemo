namespace Payments.Application.Clients;

public record ChargeRequest(Guid OrderId, decimal Amount, string Currency, string CardToken, string? ProviderBehavior = null);

public record ChargeResult(bool Success, string? TransactionId, string? ErrorMessage);

public interface IPaymentProviderClient
{
    Task<ChargeResult> ChargeAsync(ChargeRequest request, CancellationToken cancellationToken = default);
}
