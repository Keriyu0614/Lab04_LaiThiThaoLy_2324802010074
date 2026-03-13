using Messaging.Common.Events;

namespace PaymentServices.Contract.Messaging
{
    /// <summary>
    /// [INTERFACE] Contract cho Payment handler khi nhận OrderPlacedEvent
    /// Được implement bởi: PaymentPlacedHandler (trong PaymentService.Application)
    /// </summary>
    public interface IPayment
    {
        Task HandleAsync(OrderPlacedEvent evt);
    }
}
