namespace Messaging.Common.Models
{
    /// <summary>[ABSTRACT CLASS] Base cho tất cả events - chứa metadata chung</summary>
    public abstract class EventBase
    {
        public Guid EventId { get; private set; } = Guid.NewGuid();
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
        public string? CorrelationId { get; set; }
    }
}
