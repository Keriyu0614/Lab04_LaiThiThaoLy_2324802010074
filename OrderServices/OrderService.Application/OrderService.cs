using Messaging.Common.Events;
using OrderServices.Contracts.Messaging;

namespace OrderServices.OrderService.Application
{
    /// [CLASS] Application Service - Business logic xử lý đơn hàng
    /// Sau khi tạo order COD → publish OrderPlacedEvent lên RabbitMQ
    /// Sau khi confirm online payment → publish OrderPlacedEvent lên RabbitMQ
    public class OrderService
    {
        private readonly IOrderEventPublisher _publisher;

        // In-memory store (thay bằng DbContext thực tế)
        private static readonly List<OrderRecord> _orders = new();

        public OrderService(IOrderEventPublisher publisher)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        public async Task<OrderRecord> CreateOrderAsync(CreateOrderRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (!request.Items.Any()) throw new ArgumentException("Order must have at least one item.");

            var orderId = Guid.NewGuid();
            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{orderId.ToString()[..8].ToUpper()}";

            var order = new OrderRecord
            {
                Id = orderId,
                OrderNumber = orderNumber,
                UserId = request.UserId,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                PhoneNumber = request.PhoneNumber,
                PaymentMethod = request.PaymentMethod,
                Status = request.PaymentMethod == "COD" ? "Confirmed" : "Pending",
                TotalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice),
                CreatedAt = DateTime.UtcNow,
                Items = request.Items.ToList()
            };

            _orders.Add(order);

            // *** Publish event cho COD (ngay lập tức confirmed) ***
            if (request.PaymentMethod == "COD")
                await PublishEventAsync(order);

            return order;
        }

        public async Task<bool> ConfirmOrderAsync(Guid orderId)
        {
            var order = _orders.FirstOrDefault(o => o.Id == orderId)
                ?? throw new KeyNotFoundException($"Order {orderId} not found.");

            if (order.Status != "Pending")
                throw new InvalidOperationException("Order is not in Pending state.");

            order.Status = "Confirmed";

            // *** Publish event sau khi online payment thành công ***
            await PublishEventAsync(order);
            return true;
        }

        public Task<OrderRecord?> GetOrderByIdAsync(Guid orderId)
            => Task.FromResult(_orders.FirstOrDefault(o => o.Id == orderId));

        public Task<List<OrderRecord>> GetOrdersByUserAsync(Guid userId)
            => Task.FromResult(_orders.Where(o => o.UserId == userId).ToList());

        private async Task PublishEventAsync(OrderRecord order)
        {
            var evt = new OrderPlacedEvent
            {
                OrderId      = order.Id,
                OrderNumber  = order.OrderNumber,
                UserId       = order.UserId,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                PhoneNumber  = order.PhoneNumber,
                TotalAmount  = order.TotalAmount,
                Items = order.Items.Select(i => new OrderItemLine
                {
                    ProductId = i.ProductId,
                    Quantity  = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
            await _publisher.PublishOrderPlacedAsync(evt, Guid.NewGuid().ToString());
        }
    }

    public class OrderRecord
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public Guid UserId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
        public string Status { get; set; } = "Pending";
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemRequest> Items { get; set; } = new();
    }

    public class CreateOrderRequest
    {
        public Guid UserId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string PaymentMethod { get; set; } = "COD";  
        public List<OrderItemRequest> Items { get; set; } = new();
    }

    public class OrderItemRequest
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
