using Messaging.Common.Options;
using RabbitMQ.Client;

namespace Messaging.Common.Topology
{
    /// <summary>
    /// [STATIC CLASS] Bootstrapper - đảm bảo exchange/queue/binding tồn tại khi service khởi động
    /// Gọi EnsureAll() trong constructor của Publisher/Consumer (idempotent = an toàn gọi nhiều lần)
    /// </summary>
    public static class RabbitTopology
    {
        public static void EnsureAll(IModel channel, RabbitMqOptions opt)
        {
            // 1. Main topic exchange
            channel.ExchangeDeclare(opt.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);

            // 2. Dead Letter Exchange + Queue (safety net cho failed messages)
            if (!string.IsNullOrWhiteSpace(opt.DlxExchangeName))
            {
                channel.ExchangeDeclare(opt.DlxExchangeName!, ExchangeType.Fanout, durable: true, autoDelete: false);
                if (!string.IsNullOrWhiteSpace(opt.DlxQueueName))
                {
                    channel.QueueDeclare(opt.DlxQueueName!, durable: true, exclusive: false, autoDelete: false, arguments: null);
                    channel.QueueBind(opt.DlxQueueName, opt.DlxExchangeName!, routingKey: "");
                }
            }

            // 3. Queue arguments: failed messages → DLX
            var args = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(opt.DlxExchangeName))
                args["x-dead-letter-exchange"] = opt.DlxExchangeName!;

            // 4. Business queues (durable - survive restart)
            channel.QueueDeclare(opt.ProductOrderPlacedQueue, durable: true, exclusive: false, autoDelete: false, arguments: args);
            channel.QueueDeclare(opt.NotificationOrderPlacedQueue, durable: true, exclusive: false, autoDelete: false, arguments: args);

            // 5. Bind cả 2 queue với routing key "order.placed"
            //    OrderService publish "order.placed" → cả ProductService + NotificationService đều nhận
            channel.QueueBind(opt.ProductOrderPlacedQueue, opt.ExchangeName, routingKey: "order.placed");
            channel.QueueBind(opt.NotificationOrderPlacedQueue, opt.ExchangeName, routingKey: "order.placed");
        }
    }
}
