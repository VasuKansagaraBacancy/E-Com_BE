using E_Commerce.Core.DTOs;

namespace E_Commerce.Core.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<bool> UpdateUserStatusAsync(int userId, bool isActive);
    }
}

