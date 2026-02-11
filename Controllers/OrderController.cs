using E_Commerce.Core.DTOs;
using E_Commerce.Core.Exceptions;
using E_Commerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_Commerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All order operations require authentication
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        private string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }

        /// <summary>
        /// Get current user's orders
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<OrderDto>>>> GetUserOrders()
        {
            try
            {
                var userId = GetUserId();
                var orders = await _orderService.GetUserOrdersAsync(userId);

                return Ok(new ApiResponseDto<IEnumerable<OrderDto>>
                {
                    Success = true,
                    Message = "Orders retrieved successfully",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user orders");
                return StatusCode(500, new ApiResponseDto<IEnumerable<OrderDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving orders"
                });
            }
        }

        /// <summary>
        /// Get all orders (Admin only)
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<OrderDto>>>> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();

                return Ok(new ApiResponseDto<IEnumerable<OrderDto>>
                {
                    Success = true,
                    Message = "Orders retrieved successfully",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders");
                return StatusCode(500, new ApiResponseDto<IEnumerable<OrderDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving orders"
                });
            }
        }

        /// <summary>
        /// Get order by ID (User can see own orders, Admin can see all)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<OrderDto>>> GetOrderById(int id)
        {
            try
            {
                var userId = GetUserId();
                var userRole = GetUserRole();

                var order = await _orderService.GetOrderByIdAsync(id, userId, userRole);

                if (order == null)
                {
                    return NotFound(new ApiResponseDto<OrderDto>
                    {
                        Success = false,
                        Message = "Order not found"
                    });
                }

                return Ok(new ApiResponseDto<OrderDto>
                {
                    Success = true,
                    Message = "Order retrieved successfully",
                    Data = order
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponseDto<OrderDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", id);
                return StatusCode(500, new ApiResponseDto<OrderDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving order"
                });
            }
        }

        /// <summary>
        /// Create order from current user's cart
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<OrderDto>>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<OrderDto>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var userId = GetUserId();
                var order = await _orderService.CreateOrderFromCartAsync(createOrderDto, userId);

                return Ok(new ApiResponseDto<OrderDto>
                {
                    Success = true,
                    Message = "Order created successfully",
                    Data = order
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponseDto<OrderDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new ApiResponseDto<OrderDto>
                {
                    Success = false,
                    Message = "An error occurred while creating order"
                });
            }
        }

        /// <summary>
        /// Update order status (Admin only)
        /// </summary>
        [HttpPut("status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<object>>> UpdateOrderStatus([FromBody] UpdateOrderStatusDto updateStatusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var adminUserId = GetUserId();
                var result = await _orderService.UpdateOrderStatusAsync(updateStatusDto.OrderId, updateStatusDto.Status, adminUserId);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Order not found"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Order status updated successfully"
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for OrderId={OrderId}", updateStatusDto.OrderId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while updating order status"
                });
            }
        }
    }
}


