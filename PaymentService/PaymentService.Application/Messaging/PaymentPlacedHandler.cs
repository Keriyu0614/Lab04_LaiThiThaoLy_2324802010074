using Messaging.Common.Events;
using PaymentServices.Contract.Messaging;

namespace PaymentService.PaymentService.Application.Messaging
{
    /// [CLASS] Application handler - xử lý OrderPlacedEvent trong PaymentService
    /// Flow: RabbitMQ → OrderPlacedConsumer (Infrastructure) → PaymentPlacedHandler (Application)
    /// Implements: IPayment (từ PaymentServices.Contract)
    public class PaymentPlacedHandler : IPayment
    {
        private static readonly List<PaymentRecord> _payments = new();

        public Task HandleAsync(OrderPlacedEvent evt)
        {
            Console.WriteLine($"[PaymentService] Handling OrderPlacedEvent: OrderId={evt.OrderId}, Amount={evt.TotalAmount:C}");

            var payment = new PaymentRecord
            {
                Id = Guid.NewGuid(),
                OrderId = evt.OrderId,
                UserId = evt.UserId,
                Amount = evt.TotalAmount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _payments.Add(payment);

            Console.WriteLine($"[PaymentService] Payment record created: PaymentId={payment.Id}, OrderId={evt.OrderId}");
            return Task.CompletedTask;
        }

        public static List<PaymentRecord> GetAll() => _payments;
    }

    public class PaymentRecord
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
    }
}
