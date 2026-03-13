using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Messaging.Common.Publishing
{
    /// <summary>[CLASS] Generic publisher - gửi bất kỳ message T nào lên RabbitMQ exchange</summary>
    public class Publisher
    {
        private readonly IModel _channel;

        public Publisher(IModel channel) => _channel = channel;

        public void Publish<T>(string exchange, string routingKey, T message, string? correlationId = null)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var props = _channel.CreateBasicProperties();
            props.Persistent = true;    // Lưu disk, không mất khi broker restart
            props.CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            _channel.BasicPublish(exchange, routingKey, props, body);
        }
    }
}
