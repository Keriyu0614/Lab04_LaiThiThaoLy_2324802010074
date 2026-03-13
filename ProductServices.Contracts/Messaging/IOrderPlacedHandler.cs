using Messaging.Common.Events;

namespace ProductServices.Contracts.Messaging
{
    /// <summary>
    /// [INTERFACE] Contract cho handler giảm stock khi nhận OrderPlacedEvent
    /// Được implement bởi: OrderPlacedHandler (trong ProductService.Application)
    /// </summary>
    public interface IOrderPlacedHandler
    {
        Task HandleAsync(OrderPlacedEvent evt);
    }
}
