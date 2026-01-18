using E_Commerce.Core.DTOs;

namespace E_Commerce.Core.Interfaces
{
    public interface IProductCategoryService
    {
        Task<IEnumerable<ProductCategoryDto>> GetAllCategoriesAsync();
        Task<ProductCategoryDto?> GetCategoryByIdAsync(int id);
        Task<ProductCategoryDto> CreateCategoryAsync(CreateProductCategoryDto createDto);
        Task<ProductCategoryDto?> UpdateCategoryAsync(int id, UpdateProductCategoryDto updateDto);
        Task<bool> DeleteCategoryAsync(int id);
    }
}

