using System.Text.Json;
using Messaging.Common.Events;
using Messaging.Common.Options;
using Messaging.Common.Topology;
using Microsoft.Extensions.Options;
using OrderServices.Contracts.Messaging;
using RabbitMQ.Client;

namespace OrderServices.OrderService.Infrastructure.Messaging
{
    /// [CLASS] Implementation của IOrderEventPublisher
    /// Publish OrderPlacedEvent → exchange "ecommerce.topic", routing key "order.placed"
    /// Message sẽ được route tới: product.order_placed + notification.order_placed
    /// Package: RabbitMQ.Client 6.5.0, Microsoft.Extensions.Options
    public sealed class RabbitMqOrderEventPublisher : IOrderEventPublisher
    {
        private readonly IModel _channel;
        private readonly RabbitMqOptions _opt;

        public RabbitMqOrderEventPublisher(IModel channel, IOptions<RabbitMqOptions> opt)
        {
            _channel = channel;
            _opt = opt.Value;
            RabbitTopology.EnsureAll(_channel, _opt);
        }

        public Task PublishOrderPlacedAsync(OrderPlacedEvent evt, string? correlationId = null)
        {
            evt.CorrelationId = correlationId ?? evt.CorrelationId;

            var body = JsonSerializer.SerializeToUtf8Bytes(evt);

            var props = _channel.CreateBasicProperties();
            props.Persistent = true;                   
            props.CorrelationId = evt.CorrelationId;  

            _channel.BasicPublish(
                exchange: _opt.ExchangeName,           
                routingKey: "order.placed",
                mandatory: true,
                basicProperties: props,
                body: body);

            Console.WriteLine($"[OrderService ✓] Published OrderPlacedEvent | OrderId={evt.OrderId} | CorrelationId={evt.CorrelationId}");
            return Task.CompletedTask;
        }
    }
}
