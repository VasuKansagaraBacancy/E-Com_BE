using E_Commerce.Core.Entities;

namespace E_Commerce.Core.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetApprovedProductsAsync();
        Task<IEnumerable<Product>> GetPendingProductsAsync();
        Task<IEnumerable<Product>> GetBySellerIdAsync(int sellerId);
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}

