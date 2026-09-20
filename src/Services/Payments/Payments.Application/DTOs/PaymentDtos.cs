using Payments.Domain.Enums;

namespace Payments.Application.DTOs;

public record PaymentDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    string? TransactionId,
    string? FailureReason,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ProcessPaymentRequest(
    Guid OrderId,
    decimal Amount,
    string Currency,
    string CardToken,
    string? ProviderBehavior = null);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public static PagedResult<T> Create(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
        => new(items, totalCount, page, pageSize);
}
