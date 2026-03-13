namespace Messaging.Common.Options
{
    /// <summary>[CLASS] POCO mapping cấu hình RabbitMQ từ appsettings.json → "RabbitMq" section</summary>
    public sealed class RabbitMqOptions
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "ecommerce_user";
        public string Password { get; set; } = "Test@1234";
        public string VirtualHost { get; set; } = "ecommerce_vhost";
        public string ExchangeName { get; set; } = "ecommerce.topic";
        public string? DlxExchangeName { get; set; } = "ecommerce.dlx";
        public string? DlxQueueName { get; set; } = "ecommerce.dlq";
        public string ProductOrderPlacedQueue { get; set; } = "product.order_placed";
        public string NotificationOrderPlacedQueue { get; set; } = "notification.order_placed";
    }
}
