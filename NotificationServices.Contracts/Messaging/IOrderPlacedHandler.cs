using Messaging.Common.Events;

namespace NotificationServices.Contracts.Messaging
{
    /// [INTERFACE] Contract cho handler tạo notification khi nhận OrderPlacedEvent
    /// Được implement bởi: OrderPlacedHandler (trong NotificationService.Application)
    public interface IOrderPlacedHandler
    {
        Task HandleAsync(OrderPlacedEvent evt);
    }
}
