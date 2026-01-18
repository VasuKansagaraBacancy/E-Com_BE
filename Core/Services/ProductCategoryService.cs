using E_Commerce.Core.DTOs;
using E_Commerce.Core.Entities;
using E_Commerce.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Core.Services
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IProductCategoryRepository _categoryRepository;
        private readonly ILogger<ProductCategoryService> _logger;

        public ProductCategoryService(
            IProductCategoryRepository categoryRepository,
            ILogger<ProductCategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductCategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(c => new ProductCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            });
        }

        public async Task<ProductCategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return null;
            }

            return new ProductCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        public async Task<ProductCategoryDto> CreateCategoryAsync(CreateProductCategoryDto createDto)
        {
            var category = new ProductCategory
            {
                Name = createDto.Name,
                Description = createDto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            category = await _categoryRepository.CreateAsync(category);
            _logger.LogInformation("Product category created: {CategoryId}, {CategoryName}", category.Id, category.Name);

            return new ProductCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        public async Task<ProductCategoryDto?> UpdateCategoryAsync(int id, UpdateProductCategoryDto updateDto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return null;
            }

            category.Name = updateDto.Name;
            category.Description = updateDto.Description;
            category.IsActive = updateDto.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            category = await _categoryRepository.UpdateAsync(category);
            _logger.LogInformation("Product category updated: {CategoryId}, {CategoryName}", category.Id, category.Name);

            return new ProductCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var result = await _categoryRepository.DeleteAsync(id);
            if (result)
            {
                _logger.LogInformation("Product category deleted: {CategoryId}", id);
            }
            return result;
        }
    }
}

