using System.Net.Http.Json;
using Orders.Application.Clients;

namespace Orders.Infrastructure.Clients;

public class CatalogHttpClient(HttpClient httpClient) : ICatalogClient
{
    public async Task<ProductInfo?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"/api/products/{productId}", cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ProductInfo>(cancellationToken: cancellationToken);
    }
}
