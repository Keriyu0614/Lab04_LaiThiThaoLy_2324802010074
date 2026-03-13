using Microsoft.AspNetCore.Mvc;
using NotificationService.NotificationService.Application.Messaging;

namespace NotificationService.NotificationService.Controllers
{
    /// [API CONTROLLER] ASP.NET Core Web API - xử lý HTTP requests cho Notification
    /// Route: /api/notifications
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        /// GET /api/notifications
        /// Lấy tất cả notification records đã được tạo (xem kết quả consume từ RabbitMQ)
        [HttpGet]
        public IActionResult GetAll()
        {
            var notifications = OrderPlacedHandler.GetAll();
            return Ok(new
            {
                total = notifications.Count,
                data = notifications
            });
        }
        /// GET /api/notifications/{id}
        /// Lấy chi tiết một notification
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var notification = OrderPlacedHandler.GetAll().FirstOrDefault(n => n.Id == id);
            return notification == null ? NotFound() : Ok(notification);
        }
        /// GET /api/notifications/order/{orderId}
        /// Lấy notifications theo OrderId
        [HttpGet("order/{orderId}")]
        public IActionResult GetByOrderId(Guid orderId)
        {
            var notifications = OrderPlacedHandler.GetAll()
                .Where(n => n.OrderId == orderId)
                .ToList();
            return Ok(notifications);
        }
        /// GET /api/notifications/health
        /// Health check endpoint
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                service = "NotificationService",
                status = "Running",
                message = "Consuming OrderPlacedEvents from RabbitMQ → creating notifications",
                totalNotifications = OrderPlacedHandler.GetAll().Count,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
