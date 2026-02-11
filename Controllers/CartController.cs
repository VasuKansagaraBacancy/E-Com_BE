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
    [Authorize] // All cart operations require authentication
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        /// <summary>
        /// Get current user's cart items
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<CartDto>>>> GetCart()
        {
            try
            {
                var userId = GetUserId();
                var cartItems = await _cartService.GetUserCartAsync(userId);

                return Ok(new ApiResponseDto<IEnumerable<CartDto>>
                {
                    Success = true,
                    Message = "Cart retrieved successfully",
                    Data = cartItems
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart");
                return StatusCode(500, new ApiResponseDto<IEnumerable<CartDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving cart"
                });
            }
        }

        /// <summary>
        /// Add item to cart
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<CartDto>>> AddToCart([FromBody] AddToCartDto addToCartDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<CartDto>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var userId = GetUserId();
                var cartItem = await _cartService.AddToCartAsync(addToCartDto, userId);

                return Ok(new ApiResponseDto<CartDto>
                {
                    Success = true,
                    Message = "Item added to cart successfully",
                    Data = cartItem
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponseDto<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return StatusCode(500, new ApiResponseDto<CartDto>
                {
                    Success = false,
                    Message = "An error occurred while adding item to cart"
                });
            }
        }

        /// <summary>
        /// Update quantity of a cart item
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<CartDto>>> UpdateCartItem(int id, [FromBody] UpdateCartItemDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<CartDto>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var userId = GetUserId();
                var cartItem = await _cartService.UpdateCartItemAsync(id, updateDto, userId);

                if (cartItem == null)
                {
                    return NotFound(new ApiResponseDto<CartDto>
                    {
                        Success = false,
                        Message = "Cart item not found"
                    });
                }

                return Ok(new ApiResponseDto<CartDto>
                {
                    Success = true,
                    Message = "Cart item updated successfully",
                    Data = cartItem
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponseDto<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item {CartItemId}", id);
                return StatusCode(500, new ApiResponseDto<CartDto>
                {
                    Success = false,
                    Message = "An error occurred while updating cart item"
                });
            }
        }

        /// <summary>
        /// Remove a cart item
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<object>>> RemoveFromCart(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _cartService.RemoveFromCartAsync(id, userId);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Cart item not found"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Cart item removed successfully"
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
                _logger.LogError(ex, "Error removing cart item {CartItemId}", id);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while removing cart item"
                });
            }
        }

        /// <summary>
        /// Clear current user's cart
        /// </summary>
        [HttpDelete("clear")]
        public async Task<ActionResult<ApiResponseDto<object>>> ClearCart()
        {
            try
            {
                var userId = GetUserId();
                await _cartService.ClearCartAsync(userId);

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Cart cleared successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while clearing cart"
                });
            }
        }
    }
}


