using Messaging.Common.Events;
using ProductServices.Contracts.Messaging;

namespace ProductService.ProductService.Application.Messaging
{
    /// [CLASS] Application handler - giảm stock khi nhận OrderPlacedEvent
    /// Flow: RabbitMQ → OrderPlacedConsumer (Infrastructure) → OrderPlacedHandler (Application)
    /// Implements: IOrderPlacedHandler (từ ProductServices.Contracts)
    public class OrderPlacedHandler : IOrderPlacedHandler
    {
        private static readonly List<ProductStock> _products = new()
        {
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Laptop Dell XPS 15", Price = 25_000_000, Stock = 50 },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "iPhone 15 Pro Max",  Price = 32_000_000, Stock = 100 },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Sony WH-1000XM5",   Price = 8_500_000,  Stock = 200 },
        };

        public Task HandleAsync(OrderPlacedEvent evt)
        {
            Console.WriteLine($"[ProductService] Handling OrderPlacedEvent: OrderId={evt.OrderId}, Items={evt.Items.Count}");

            foreach (var item in evt.Items)
            {
                var product = _products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.Stock = Math.Max(0, product.Stock - item.Quantity);
                    Console.WriteLine($"[ProductService ✓] Stock updated: {product.Name} | -{item.Quantity} | Remaining={product.Stock}");
                }
                else
                {
                    Console.WriteLine($"[ProductService ⚠] Product not found: {item.ProductId}");
                }
            }

            return Task.CompletedTask;
        }

        public static List<ProductStock> GetAll() => _products;
    }

    public class ProductStock
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
