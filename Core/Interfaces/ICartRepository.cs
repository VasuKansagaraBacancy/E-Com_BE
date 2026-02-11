using E_Commerce.Core.Entities;

namespace E_Commerce.Core.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetByIdAsync(int id);
        Task<Cart?> GetByUserAndProductAsync(int userId, int productId);
        Task<IEnumerable<Cart>> GetByUserIdAsync(int userId);
        Task<Cart> CreateAsync(Cart cart);
        Task<Cart> UpdateAsync(Cart cart);
        Task<bool> DeleteAsync(int id);
        Task<bool> ClearUserCartAsync(int userId);
    }
}

