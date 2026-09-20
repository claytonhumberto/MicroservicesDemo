namespace Orders.Application.Clients;

public record ProductInfo(Guid Id, string Name, decimal Price, int StockQuantity, bool IsActive);

public interface ICatalogClient
{
    Task<ProductInfo?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default);
}
