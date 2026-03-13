using Messaging.Common.Models;

namespace Messaging.Common.Events
{
    /// <summary>[CLASS] Event publish khi đơn hàng được xác nhận - consume bởi Product + Notification</summary>
    public sealed class OrderPlacedEvent : EventBase
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public List<OrderItemLine> Items { get; set; } = new();
    }

    /// <summary>[CLASS] Chi tiết từng sản phẩm trong event</summary>
    public sealed class OrderItemLine
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
