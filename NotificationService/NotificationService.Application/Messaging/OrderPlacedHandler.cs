using Messaging.Common.Events;
using NotificationServices.Contracts.Messaging;

namespace NotificationService.NotificationService.Application.Messaging
{
    /// [CLASS] Application handler - tạo notification record khi nhận OrderPlacedEvent
    /// Flow: RabbitMQ → OrderPlacedConsumer (Infrastructure) → OrderPlacedHandler (Application)
    /// Implements: IOrderPlacedHandler (từ NotificationServices.Contracts)
    public class OrderPlacedHandler : IOrderPlacedHandler
    {
        private static readonly List<NotificationRecord> _notifications = new();

        public Task HandleAsync(OrderPlacedEvent evt)
        {
            Console.WriteLine($"[NotificationService] Handling OrderPlacedEvent: OrderId={evt.OrderId}");

            var notification = new NotificationRecord
            {
                Id = Guid.NewGuid(),
                OrderId = evt.OrderId,
                UserId = evt.UserId,
                CustomerName = evt.CustomerName,
                CustomerEmail = evt.CustomerEmail,
                PhoneNumber = evt.PhoneNumber,
                OrderNumber = evt.OrderNumber,
                TotalAmount = evt.TotalAmount,
                Channel = "Email",
                Status = "Sent",
                Message = BuildMessage(evt),
                CreatedAt = DateTime.UtcNow
            };

            _notifications.Add(notification);

            Console.WriteLine($"[NotificationService ✓] Notification created: Id={notification.Id}");
            Console.WriteLine($"[NotificationService ✓] → To: {evt.CustomerEmail}");
            Console.WriteLine($"[NotificationService ✓] → Subject: Order Confirmed #{evt.OrderNumber}");
            Console.WriteLine($"[NotificationService ✓] → Message: {notification.Message}");

            return Task.CompletedTask;
        }

        public static List<NotificationRecord> GetAll() => _notifications;

        private static string BuildMessage(OrderPlacedEvent evt)
        {
            var itemLines = string.Join(", ", evt.Items.Select(i =>
                $"ProductId={i.ProductId} x{i.Quantity} @ {i.UnitPrice:C}"));

            return $"Dear {evt.CustomerName}, your order #{evt.OrderNumber} has been confirmed. " +
                   $"Total: {evt.TotalAmount:C}. Items: [{itemLines}]. Thank you for shopping!";
        }
    }

    public class NotificationRecord
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string OrderNumber { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string Channel { get; set; } = "Email";
        public string Status { get; set; } = "Pending";
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
