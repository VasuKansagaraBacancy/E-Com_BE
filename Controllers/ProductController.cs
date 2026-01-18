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
    [Authorize] // Require authentication for all endpoints
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            ILogger<ProductController> logger)
        {
            _productService = productService;
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
        /// Get all products (Admin and Seller can see all, Customers see only approved)
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // Public access - but filter by role in service
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductDto>>>> GetProducts()
        {
            try
            {
                var userRole = GetUserRole();
                IEnumerable<ProductDto> products;

                // Customers and anonymous users see only approved products
                if (string.IsNullOrEmpty(userRole) || userRole == "Customer")
                {
                    products = await _productService.GetApprovedProductsAsync();
                }
                else
                {
                    // Admin and Seller see all products
                    products = await _productService.GetAllProductsAsync();
                }

                return Ok(new ApiResponseDto<IEnumerable<ProductDto>>
                {
                    Success = true,
                    Message = "Products retrieved successfully",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(500, new ApiResponseDto<IEnumerable<ProductDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving products"
                });
            }
        }

        /// <summary>
        /// Get approved products (Public access)
        /// </summary>
        [HttpGet("approved")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductDto>>>> GetApprovedProducts()
        {
            try
            {
                var products = await _productService.GetApprovedProductsAsync();

                return Ok(new ApiResponseDto<IEnumerable<ProductDto>>
                {
                    Success = true,
                    Message = "Approved products retrieved successfully",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving approved products");
                return StatusCode(500, new ApiResponseDto<IEnumerable<ProductDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving products"
                });
            }
        }

        /// <summary>
        /// Get pending products (Admin only)
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductDto>>>> GetPendingProducts()
        {
            try
            {
                var products = await _productService.GetPendingProductsAsync();

                return Ok(new ApiResponseDto<IEnumerable<ProductDto>>
                {
                    Success = true,
                    Message = "Pending products retrieved successfully",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending products");
                return StatusCode(500, new ApiResponseDto<IEnumerable<ProductDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving pending products"
                });
            }
        }

        /// <summary>
        /// Get products by seller (Seller can see their own, Admin can see any seller's)
        /// </summary>
        [HttpGet("seller/{sellerId}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductDto>>>> GetProductsBySeller(int sellerId)
        {
            try
            {
                var userId = GetUserId();
                var userRole = GetUserRole();

                // Seller can only see their own products
                if (userRole == "Seller" && userId != sellerId)
                {
                    return Forbid("You can only view your own products.");
                }

                var products = await _productService.GetProductsBySellerAsync(sellerId);

                return Ok(new ApiResponseDto<IEnumerable<ProductDto>>
                {
                    Success = true,
                    Message = "Products retrieved successfully",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for seller {SellerId}", sellerId);
                return StatusCode(500, new ApiResponseDto<IEnumerable<ProductDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving products"
                });
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<ProductDto>>> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);

                if (product == null)
                {
                    return NotFound(new ApiResponseDto<ProductDto>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                // Customers can only see approved products
                var userRole = GetUserRole();
                if ((string.IsNullOrEmpty(userRole) || userRole == "Customer") && product.Status != "Approved")
                {
                    return NotFound(new ApiResponseDto<ProductDto>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                return Ok(new ApiResponseDto<ProductDto>
                {
                    Success = true,
                    Message = "Product retrieved successfully",
                    Data = product
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product {ProductId}", id);
                return StatusCode(500, new ApiResponseDto<ProductDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving product"
                });
            }
        }

        /// <summary>
        /// Create a new product (Admin or Seller)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<ActionResult<ApiResponseDto<ProductDto>>> CreateProduct([FromBody] CreateProductDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<ProductDto>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var userId = GetUserId();
                var userRole = GetUserRole();

                var product = await _productService.CreateProductAsync(createDto, userId, userRole);

                return Ok(new ApiResponseDto<ProductDto>
                {
                    Success = true,
                    Message = userRole == "Admin" 
                        ? "Product created and approved successfully" 
                        : "Product created successfully. Waiting for admin approval.",
                    Data = product
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponseDto<ProductDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new ApiResponseDto<ProductDto>
                {
                    Success = false,
                    Message = "An error occurred while creating product"
                });
            }
        }

        /// <summary>
        /// Update product (Admin or Seller - Seller can only update their own)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<ActionResult<ApiResponseDto<ProductDto>>> UpdateProduct(int id, [FromBody] UpdateProductDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<ProductDto>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var userId = GetUserId();
                var userRole = GetUserRole();

                var product = await _productService.UpdateProductAsync(id, updateDto, userId, userRole);

                if (product == null)
                {
                    return NotFound(new ApiResponseDto<ProductDto>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                return Ok(new ApiResponseDto<ProductDto>
                {
                    Success = true,
                    Message = userRole == "Seller" && product.Status == "Pending"
                        ? "Product updated successfully. Waiting for admin approval."
                        : "Product updated successfully",
                    Data = product
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponseDto<ProductDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return StatusCode(500, new ApiResponseDto<ProductDto>
                {
                    Success = false,
                    Message = "An error occurred while updating product"
                });
            }
        }

        /// <summary>
        /// Delete product (Admin or Seller - Seller can only delete their own)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteProduct(int id)
        {
            try
            {
                var userId = GetUserId();
                var userRole = GetUserRole();

                var result = await _productService.DeleteProductAsync(id, userId, userRole);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Product deleted successfully"
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
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting product"
                });
            }
        }

        /// <summary>
        /// Approve or reject product (Admin only)
        /// </summary>
        [HttpPost("approve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<object>>> ApproveProduct([FromBody] ApproveProductDto approveDto)
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
                var result = await _productService.ApproveProductAsync(approveDto.ProductId, approveDto.Approved, adminUserId);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Product not found or cannot be approved/rejected"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = approveDto.Approved 
                        ? "Product approved successfully" 
                        : "Product rejected successfully"
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
                _logger.LogError(ex, "Error approving product {ProductId}", approveDto.ProductId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while approving product"
                });
            }
        }
    }
}

