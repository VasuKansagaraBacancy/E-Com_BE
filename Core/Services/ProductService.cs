using E_Commerce.Core.DTOs;
using E_Commerce.Core.Entities;
using E_Commerce.Core.Exceptions;
using E_Commerce.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _categoryRepository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            IProductCategoryRepository categoryRepository,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(p => MapToDto(p));
        }

        public async Task<IEnumerable<ProductDto>> GetApprovedProductsAsync()
        {
            var products = await _productRepository.GetApprovedProductsAsync();
            return products.Select(p => MapToDto(p));
        }

        public async Task<IEnumerable<ProductDto>> GetPendingProductsAsync()
        {
            var products = await _productRepository.GetPendingProductsAsync();
            return products.Select(p => MapToDto(p));
        }

        public async Task<IEnumerable<ProductDto>> GetProductsBySellerAsync(int sellerId)
        {
            var products = await _productRepository.GetBySellerIdAsync(sellerId);
            return products.Select(p => MapToDto(p));
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return null;
            }

            return MapToDto(product);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createDto, int createdByUserId, string userRole)
        {
            // Verify category exists
            var category = await _categoryRepository.GetByIdAsync(createDto.CategoryId);
            if (category == null || !category.IsActive)
            {
                throw new ValidationException("Invalid or inactive product category.");
            }

            // Determine status based on user role
            // Admin products are auto-approved, Seller products need approval
            string status = userRole == "Admin" ? "Approved" : "Pending";
            int? approvedByUserId = userRole == "Admin" ? createdByUserId : null;
            DateTime? approvedAt = userRole == "Admin" ? DateTime.UtcNow : null;

            var product = new Product
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Price = createDto.Price,
                StockQuantity = createDto.StockQuantity,
                ImageUrl = createDto.ImageUrl,
                CategoryId = createDto.CategoryId,
                CreatedByUserId = createdByUserId,
                Status = status,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ApprovedByUserId = approvedByUserId,
                ApprovedAt = approvedAt
            };

            product = await _productRepository.CreateAsync(product);
            _logger.LogInformation("Product created: {ProductId}, {ProductName}, Status: {Status}, CreatedBy: {UserId}", 
                product.Id, product.Name, product.Status, createdByUserId);

            var createdProduct = await _productRepository.GetByIdAsync(product.Id);
            return MapToDto(createdProduct!);
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateDto, int userId, string userRole)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return null;
            }

            // Check permissions: Seller can only update their own products
            if (userRole == "Seller" && product.CreatedByUserId != userId)
            {
                throw new ValidationException("You can only update your own products.");
            }

            // Verify category exists
            var category = await _categoryRepository.GetByIdAsync(updateDto.CategoryId);
            if (category == null || !category.IsActive)
            {
                throw new ValidationException("Invalid or inactive product category.");
            }

            // If seller updates a previously approved product, it needs re-approval
            if (userRole == "Seller" && product.Status == "Approved")
            {
                product.Status = "Pending";
                product.ApprovedByUserId = null;
                product.ApprovedAt = null;
            }

            product.Name = updateDto.Name;
            product.Description = updateDto.Description;
            product.Price = updateDto.Price;
            product.StockQuantity = updateDto.StockQuantity;
            product.ImageUrl = updateDto.ImageUrl;
            product.CategoryId = updateDto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            product = await _productRepository.UpdateAsync(product);
            _logger.LogInformation("Product updated: {ProductId}, {ProductName}, UpdatedBy: {UserId}", 
                product.Id, product.Name, userId);

            var updatedProduct = await _productRepository.GetByIdAsync(product.Id);
            return MapToDto(updatedProduct!);
        }

        public async Task<bool> DeleteProductAsync(int id, int userId, string userRole)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return false;
            }

            // Check permissions: Seller can only delete their own products
            if (userRole == "Seller" && product.CreatedByUserId != userId)
            {
                throw new ValidationException("You can only delete your own products.");
            }

            var result = await _productRepository.DeleteAsync(id);
            if (result)
            {
                _logger.LogInformation("Product deleted: {ProductId}, DeletedBy: {UserId}", id, userId);
            }
            return result;
        }

        public async Task<bool> ApproveProductAsync(int productId, bool approved, int adminUserId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return false;
            }

            if (product.Status != "Pending")
            {
                throw new ValidationException("Only pending products can be approved or rejected.");
            }

            product.Status = approved ? "Approved" : "Rejected";
            product.ApprovedByUserId = adminUserId;
            product.ApprovedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            _logger.LogInformation("Product {Action}: {ProductId}, AdminUserId: {AdminUserId}", 
                approved ? "approved" : "rejected", productId, adminUserId);

            return true;
        }

        private ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? "",
                CreatedByUserId = product.CreatedByUserId,
                CreatedByEmail = product.CreatedBy?.Email ?? "",
                Status = product.Status,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                ApprovedAt = product.ApprovedAt,
                ApprovedByUserId = product.ApprovedByUserId,
                ApprovedByEmail = product.ApprovedBy?.Email ?? ""
            };
        }
    }
}

