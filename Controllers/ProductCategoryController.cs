using E_Commerce.Core.DTOs;
using E_Commerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Admin only
    public class ProductCategoryController : ControllerBase
    {
        private readonly IProductCategoryService _categoryService;
        private readonly ILogger<ProductCategoryController> _logger;

        public ProductCategoryController(
            IProductCategoryService categoryService,
            ILogger<ProductCategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active categories
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // Allow public access to view categories
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductCategoryDto>>>> GetCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();

                return Ok(new ApiResponseDto<IEnumerable<ProductCategoryDto>>
                {
                    Success = true,
                    Message = "Categories retrieved successfully",
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, new ApiResponseDto<IEnumerable<ProductCategoryDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving categories"
                });
            }
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous] // Allow public access
        public async Task<ActionResult<ApiResponseDto<ProductCategoryDto>>> GetCategory(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);

                if (category == null)
                {
                    return NotFound(new ApiResponseDto<ProductCategoryDto>
                    {
                        Success = false,
                        Message = "Category not found"
                    });
                }

                return Ok(new ApiResponseDto<ProductCategoryDto>
                {
                    Success = true,
                    Message = "Category retrieved successfully",
                    Data = category
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
                return StatusCode(500, new ApiResponseDto<ProductCategoryDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving category"
                });
            }
        }

        /// <summary>
        /// Create a new category (Admin only)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<ProductCategoryDto>>> CreateCategory([FromBody] CreateProductCategoryDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<ProductCategoryDto>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var category = await _categoryService.CreateCategoryAsync(createDto);

                return Ok(new ApiResponseDto<ProductCategoryDto>
                {
                    Success = true,
                    Message = "Category created successfully",
                    Data = category
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new ApiResponseDto<ProductCategoryDto>
                {
                    Success = false,
                    Message = "An error occurred while creating category"
                });
            }
        }

        /// <summary>
        /// Update category (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<ProductCategoryDto>>> UpdateCategory(int id, [FromBody] UpdateProductCategoryDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<ProductCategoryDto>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var category = await _categoryService.UpdateCategoryAsync(id, updateDto);

                if (category == null)
                {
                    return NotFound(new ApiResponseDto<ProductCategoryDto>
                    {
                        Success = false,
                        Message = "Category not found"
                    });
                }

                return Ok(new ApiResponseDto<ProductCategoryDto>
                {
                    Success = true,
                    Message = "Category updated successfully",
                    Data = category
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", id);
                return StatusCode(500, new ApiResponseDto<ProductCategoryDto>
                {
                    Success = false,
                    Message = "An error occurred while updating category"
                });
            }
        }

        /// <summary>
        /// Delete category (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteCategory(int id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Category not found"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Category deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting category"
                });
            }
        }
    }
}

