using System.Net.Http.Json;
using Payments.Application.Clients;

namespace Payments.Infrastructure.Clients;

public class FakePaymentProviderClient(HttpClient httpClient) : IPaymentProviderClient
{
    public async Task<ChargeResult> ChargeAsync(ChargeRequest request, CancellationToken cancellationToken = default)
    {
        // The ProviderBehavior field controls the FakeProvider's response: success|fail|timeout|slow.
        // This is intentional for resilience demos — in production this field would not exist.
        var behavior = request.ProviderBehavior ?? "success";
        var url = $"/charge?behavior={behavior}";

        var payload = new
        {
            OrderId = request.OrderId,
            Amount = request.Amount,
            Currency = request.Currency,
            CardToken = request.CardToken
        };

        var response = await httpClient.PostAsJsonAsync(url, payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return new ChargeResult(false, null, $"Provider returned {(int)response.StatusCode}: {errorContent}");
        }

        var result = await response.Content.ReadFromJsonAsync<ProviderResponse>(cancellationToken: cancellationToken);
        return new ChargeResult(true, result?.TransactionId.ToString(), null);
    }

    private record ProviderResponse(Guid TransactionId, string Status, decimal Amount, DateTime ProcessedAt);
}
