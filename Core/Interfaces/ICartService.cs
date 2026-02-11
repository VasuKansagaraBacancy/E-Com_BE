using E_Commerce.Core.DTOs;

namespace E_Commerce.Core.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<CartDto>> GetUserCartAsync(int userId);
        Task<CartDto> AddToCartAsync(AddToCartDto addToCartDto, int userId);
        Task<CartDto?> UpdateCartItemAsync(int cartItemId, UpdateCartItemDto updateDto, int userId);
        Task<bool> RemoveFromCartAsync(int cartItemId, int userId);
        Task<bool> ClearCartAsync(int userId);
        Task<decimal> GetCartTotalAsync(int userId);
    }
}

