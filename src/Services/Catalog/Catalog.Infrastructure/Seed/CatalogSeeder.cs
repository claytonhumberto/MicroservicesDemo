using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Seed;

public static class CatalogSeeder
{
    public static async Task SeedAsync(CatalogDbContext context)
    {
        if (context.Products.Any()) return;

        var products = new[]
        {
            Product.Create("Laptop Pro 15", "High-performance laptop with 16GB RAM and 512GB SSD", 1299.99m, 50, "Electronics"),
            Product.Create("Wireless Mouse", "Ergonomic wireless mouse with long battery life", 29.99m, 200, "Electronics"),
            Product.Create("Mechanical Keyboard", "Tactile mechanical keyboard with RGB backlight", 89.99m, 150, "Electronics"),
            Product.Create("4K Monitor", "27-inch 4K UHD monitor with HDR support", 449.99m, 30, "Electronics"),
            Product.Create("USB-C Hub", "7-in-1 USB-C hub with HDMI, USB 3.0 and SD card reader", 49.99m, 100, "Electronics"),
            Product.Create("Running Shoes", "Lightweight and breathable running shoes", 79.99m, 75, "Sports"),
            Product.Create("Yoga Mat", "Non-slip premium yoga mat, 6mm thickness", 34.99m, 120, "Sports"),
            Product.Create("Water Bottle", "Insulated stainless steel water bottle, 1L", 24.99m, 300, "Sports"),
            Product.Create("Resistance Bands Set", "Set of 5 resistance bands for strength training", 19.99m, 250, "Sports"),
            Product.Create("Jump Rope", "Speed jump rope with ball bearings", 14.99m, 180, "Sports"),
            Product.Create("The Clean Coder", "A code of conduct for professional programmers by Robert Martin", 39.99m, 60, "Books"),
            Product.Create("Designing Data-Intensive Applications", "The big ideas behind reliable, scalable and maintainable systems", 54.99m, 45, "Books"),
            Product.Create("Clean Architecture", "A craftsman's guide to software structure and design", 44.99m, 55, "Books"),
            Product.Create("Microservices Patterns", "With examples in Java by Chris Richardson", 49.99m, 40, "Books"),
            Product.Create("The Pragmatic Programmer", "Your journey to mastery, 20th anniversary edition", 42.99m, 65, "Books"),
            Product.Create("Standing Desk", "Electric height-adjustable standing desk", 599.99m, 20, "Office"),
            Product.Create("Ergonomic Chair", "Lumbar support office chair with adjustable armrests", 399.99m, 15, "Office"),
            Product.Create("Desk Lamp", "LED desk lamp with adjustable brightness and color temperature", 49.99m, 80, "Office"),
            Product.Create("Webcam HD", "1080p HD webcam with built-in microphone", 79.99m, 90, "Office"),
            Product.Create("Headset Pro", "Noise-cancelling USB headset for calls and music", 99.99m, 70, "Office"),
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}
