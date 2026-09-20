using BuildingBlocks.Contracts.Events;
using MassTransit;
using Orders.Application.Clients;
using Orders.Application.DTOs;
using Orders.Domain.Entities;
using Orders.Domain.Repositories;

namespace Orders.Application.Services;

public class OrderService(
    IOrderRepository orderRepository,
    ICatalogClient catalogClient,
    IPublishEndpoint publishEndpoint)
{
    public async Task<PagedResult<OrderDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var (items, totalCount) = await orderRepository.GetPagedAsync(page, pageSize, cancellationToken);
        var dtos = items.Select(MapToDto).ToList();
        return PagedResult<OrderDto>.Create(dtos, totalCount, page, pageSize);
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : MapToDto(order);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items is null || request.Items.Count == 0)
            throw new ArgumentException("An order must have at least one item.");

        var order = Order.Create(request.CustomerName, request.CustomerEmail);

        foreach (var itemRequest in request.Items)
        {
            var product = await catalogClient.GetProductAsync(itemRequest.ProductId, cancellationToken)
                ?? throw new KeyNotFoundException($"Product '{itemRequest.ProductId}' was not found in Catalog.");

            if (!product.IsActive)
                throw new InvalidOperationException($"Product '{product.Name}' is not available for purchase.");

            if (product.StockQuantity < itemRequest.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}, Requested: {itemRequest.Quantity}.");

            order.AddItem(product.Id, product.Name, product.Price, itemRequest.Quantity);
        }

        await orderRepository.AddAsync(order, cancellationToken);
        return MapToDto(order);
    }

    public async Task<OrderDto> ConfirmAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Order '{id}' was not found.");

        order.Confirm();
        await orderRepository.UpdateAsync(order, cancellationToken);

        await publishEndpoint.Publish(new OrderConfirmed(
            OrderId: order.Id,
            CustomerName: order.CustomerName,
            CustomerEmail: order.CustomerEmail,
            TotalAmount: order.TotalAmount,
            ConfirmedAt: order.UpdatedAt), cancellationToken);

        return MapToDto(order);
    }

    public async Task<OrderDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Order '{id}' was not found.");

        order.Cancel();
        await orderRepository.UpdateAsync(order, cancellationToken);
        return MapToDto(order);
    }

    private static OrderDto MapToDto(Order order) => new(
        order.Id,
        order.CustomerName,
        order.CustomerEmail,
        order.Status,
        order.TotalAmount,
        order.CreatedAt,
        order.UpdatedAt,
        order.Items.Select(i => new OrderItemDto(
            i.Id, i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.Subtotal)).ToList());
}
