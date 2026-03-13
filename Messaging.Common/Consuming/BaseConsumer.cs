using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Messaging.Common.Consuming
{
    /// <summary>
    /// [ABSTRACT CLASS] Base reusable consumer
    /// Xử lý ACK/NACK tự động, subclass chỉ cần implement HandleMessage()
    /// </summary>
    public abstract class BaseConsumer<T>
    {
        private readonly IModel _channel;
        private readonly string _queue;

        protected BaseConsumer(IModel channel, string queue)
        {
            _channel = channel;
            _queue = queue;
        }

        public void Start()
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                try
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<T>(body);
                    await HandleMessage(message!, ea.BasicProperties.CorrelationId);
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BaseConsumer Error] {ex.Message}");
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };
            _channel.BasicConsume(_queue, autoAck: false, consumer);
        }

        protected abstract Task HandleMessage(T message, string correlationId);
    }
}
