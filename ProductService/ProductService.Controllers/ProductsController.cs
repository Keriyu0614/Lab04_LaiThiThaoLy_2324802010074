using Microsoft.AspNetCore.Mvc;
using ProductService.ProductService.Application.Messaging;

namespace ProductService.ProductService.Controllers
{
    /// [API CONTROLLER] ASP.NET Core Web API - xử lý HTTP requests cho Product
    /// Route: /api/products
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        /// GET /api/products
        /// Lấy danh sách sản phẩm và stock hiện tại
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(OrderPlacedHandler.GetAll());
        }

        /// GET /api/products/{id}
        /// Lấy thông tin một sản phẩm
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var product = OrderPlacedHandler.GetAll().FirstOrDefault(p => p.Id == id);
            return product == null ? NotFound() : Ok(product);
        }

        /// GET /api/products/health
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                service = "ProductService",
                status = "Running",
                message = "Consuming OrderPlacedEvents from RabbitMQ → decreasing stock",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
