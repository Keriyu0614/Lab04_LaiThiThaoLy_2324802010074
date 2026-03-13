using Microsoft.AspNetCore.Mvc;
using PaymentService.PaymentService.Application.Messaging;

namespace PaymentService.PaymentService.Controllers
{
    /// [API CONTROLLER] ASP.NET Core Web API - xử lý HTTP requests cho Payment
    /// Route: /api/payments
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        /// GET /api/payments - Lấy tất cả payment records (demo)
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(PaymentPlacedHandler.GetAll());
        }

        /// GET /api/payments/health - Health check
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                service = "PaymentService",
                status = "Running",
                message = "Consuming OrderPlacedEvents from RabbitMQ",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
