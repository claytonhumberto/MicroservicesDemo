using Catalog.Application.DTOs;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;

namespace Catalog.Application.Services;

public class ProductService(IProductRepository repository)
{
    public async Task<PagedResult<ProductDto>> GetProductsAsync(int page, int pageSize, string? category, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetPagedAsync(page, pageSize, category, cancellationToken);
        var dtos = items.Select(MapToDto).ToList();
        return PagedResult<ProductDto>.Create(dtos, page, pageSize, totalCount);
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : MapToDto(product);
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken)
        => await repository.GetCategoriesAsync(cancellationToken);

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Description, request.Price, request.StockQuantity, request.Category);
        await repository.AddAsync(product, cancellationToken);
        return MapToDto(product);
    }

    public async Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        if (product is null) return null;

        product.Update(request.Name, request.Description, request.Price, request.StockQuantity, request.Category);
        await repository.UpdateAsync(product, cancellationToken);
        return MapToDto(product);
    }

    public async Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        if (product is null) return false;

        product.Deactivate();
        await repository.UpdateAsync(product, cancellationToken);
        return true;
    }

    private static ProductDto MapToDto(Product p) => new(
        p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.Category, p.IsActive, p.CreatedAt, p.UpdatedAt);
}
