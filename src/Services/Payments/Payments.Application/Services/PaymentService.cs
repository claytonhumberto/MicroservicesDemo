using Payments.Application.Clients;
using Payments.Application.DTOs;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;

namespace Payments.Application.Services;

public class PaymentService(IPaymentRepository paymentRepository, IPaymentProviderClient providerClient)
{
    public async Task<PagedResult<PaymentDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var (items, totalCount) = await paymentRepository.GetPagedAsync(page, pageSize, cancellationToken);
        return PagedResult<PaymentDto>.Create(items.Select(MapToDto).ToList(), totalCount, page, pageSize);
    }

    public async Task<PaymentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.GetByIdAsync(id, cancellationToken);
        return payment is null ? null : MapToDto(payment);
    }

    public async Task<IReadOnlyList<PaymentDto>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var payments = await paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        return payments.Select(MapToDto).ToList();
    }

    public async Task<PaymentDto> ProcessAsync(ProcessPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var payment = Payment.Create(request.OrderId, request.Amount, request.Currency, request.CardToken);
        await paymentRepository.AddAsync(payment, cancellationToken);

        ChargeResult result;
        try
        {
            var chargeRequest = new ChargeRequest(request.OrderId, request.Amount, request.Currency, request.CardToken, request.ProviderBehavior);
            result = await providerClient.ChargeAsync(chargeRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            // Provider unreachable or circuit breaker open — record failure without re-throwing
            payment.Fail($"Provider communication error: {ex.Message}");
            await paymentRepository.UpdateAsync(payment, cancellationToken);
            return MapToDto(payment);
        }

        if (result.Success)
            payment.Approve(result.TransactionId!);
        else
            payment.Decline(result.ErrorMessage ?? "Payment declined by provider");

        await paymentRepository.UpdateAsync(payment, cancellationToken);
        return MapToDto(payment);
    }

    private static PaymentDto MapToDto(Payment p) => new(
        p.Id, p.OrderId, p.Amount, p.Currency, p.Status,
        p.TransactionId, p.FailureReason, p.CreatedAt, p.UpdatedAt);
}
