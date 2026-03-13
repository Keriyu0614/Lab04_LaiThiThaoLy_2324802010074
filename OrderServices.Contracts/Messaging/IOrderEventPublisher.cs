using Messaging.Common.Events;

namespace OrderServices.Contracts.Messaging
{
    /// [INTERFACE] Contract cho việc publish OrderPlacedEvent
    /// Được implement bởi: RabbitMqOrderEventPublisher (trong OrderServices.Infrastructure)
    public interface IOrderEventPublisher
    {
        Task PublishOrderPlacedAsync(OrderPlacedEvent evt, string? correlationId = null);
    }
}
