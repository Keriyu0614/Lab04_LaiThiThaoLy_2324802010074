using Messaging.Common.Events;
using Messaging.Common.Options;
using Messaging.Common.Topology;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationServices.Contracts.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace NotificationService.NotificationService.Infrastructure.Messaging
{
    /// <summary>
    /// [CLASS] BackgroundService - lắng nghe queue "notification.order_placed"
    /// Khi nhận OrderPlacedEvent → gọi IOrderPlacedHandler để tạo notification
    /// Inherits: BackgroundService (ASP.NET Core IHostedService)
    /// Package: RabbitMQ.Client 6.5.0, Microsoft.Extensions.Hosting
    /// </summary>
    public sealed class OrderPlacedConsumer : BackgroundService
    {
        private readonly ILogger<OrderPlacedConsumer> _logger;
        private readonly IModel _channel;
        private readonly RabbitMqOptions _options;
        private readonly IServiceScopeFactory _scopeFactory;

        public OrderPlacedConsumer(
            ILogger<OrderPlacedConsumer> logger,
            IModel channel,
            IOptions<RabbitMqOptions> options,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _channel = channel;
            _options = options.Value;
            _scopeFactory = scopeFactory;
            // Đảm bảo topology tồn tại trước khi consume
            RabbitTopology.EnsureAll(_channel, _options);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // QoS: tối đa 10 message chưa ACK cùng lúc → tránh flood consumer
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (_, ea) =>
            {
                try
                {
                    // Deserialize JSON body → OrderPlacedEvent
                    var evt = JsonSerializer.Deserialize<OrderPlacedEvent>(
                        ea.Body.Span,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (evt == null)
                    {
                        _logger.LogWarning("[NotificationService] Null event received → DLQ");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                        return;
                    }

                    // Tạo DI scope: BackgroundService là Singleton,
                    // IOrderPlacedHandler có thể là Scoped (DbContext, Email sender...)
                    using var scope = _scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<IOrderPlacedHandler>();

                    // Gọi business logic: tạo notification record + gửi email
                    await handler.HandleAsync(evt);

                    // ACK: báo RabbitMQ xử lý thành công → xóa message khỏi queue
                    _channel.BasicAck(ea.DeliveryTag, false);
                    _logger.LogInformation("[NotificationService ✓] ACK | OrderId={OrderId} | Email={Email}",
                        evt.OrderId, evt.CustomerEmail);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[NotificationService ✗] Failed to process OrderPlacedEvent");
                    // NACK: lỗi → đẩy sang DLQ để điều tra sau (requeue=false)
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            // Bắt đầu consume từ notification queue, manual ACK
            _channel.BasicConsume(
                queue: _options.NotificationOrderPlacedQueue,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("[NotificationService] Consumer started → queue: {Queue}",
                _options.NotificationOrderPlacedQueue);

            return Task.CompletedTask;
        }
    }
}
