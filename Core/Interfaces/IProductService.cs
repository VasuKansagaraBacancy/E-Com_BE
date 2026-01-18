using E_Commerce.Core.DTOs;

namespace E_Commerce.Core.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductDto>> GetApprovedProductsAsync();
        Task<IEnumerable<ProductDto>> GetPendingProductsAsync();
        Task<IEnumerable<ProductDto>> GetProductsBySellerAsync(int sellerId);
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(CreateProductDto createDto, int createdByUserId, string userRole);
        Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateDto, int userId, string userRole);
        Task<bool> DeleteProductAsync(int id, int userId, string userRole);
        Task<bool> ApproveProductAsync(int productId, bool approved, int adminUserId);
    }
}

