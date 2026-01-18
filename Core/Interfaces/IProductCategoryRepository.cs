using E_Commerce.Core.Entities;

namespace E_Commerce.Core.Interfaces
{
    public interface IProductCategoryRepository
    {
        Task<ProductCategory?> GetByIdAsync(int id);
        Task<IEnumerable<ProductCategory>> GetAllAsync();
        Task<ProductCategory> CreateAsync(ProductCategory category);
        Task<ProductCategory> UpdateAsync(ProductCategory category);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}

