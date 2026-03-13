using Microsoft.AspNetCore.Mvc;
using OrderServices.OrderService.Application;

namespace OrderServices.OrderService.Controllers
{
    /// [API CONTROLLER] ASP.NET Core Web API - xử lý HTTP requests cho Order
    /// Route: /api/orders
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService.Application.OrderService _orderService;

        public OrdersController(OrderService.Application.OrderService orderService)
        {
            _orderService = orderService;
        }
        /// POST /api/orders
        /// Tạo đơn hàng. COD → publish RabbitMQ ngay. Online → chờ confirm.
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
            }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }
        /// POST /api/orders/{id}/confirm
        /// Confirm thanh toán online → publish RabbitMQ event
        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmOrder(Guid id)
        {
            try
            {
                await _orderService.ConfirmOrderAsync(id);
                return Ok(new { message = "Order confirmed. RabbitMQ event published.", orderId = id });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }

        /// GET /api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            return order == null ? NotFound() : Ok(order);
        }

        /// GET /api/orders/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            var orders = await _orderService.GetOrdersByUserAsync(userId);
            return Ok(orders);
        }
    }
}
