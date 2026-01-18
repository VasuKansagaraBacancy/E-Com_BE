using E_Commerce.Core.DTOs;
using E_Commerce.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            
            return users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                HasGoogleAuth = !string.IsNullOrEmpty(u.GoogleId),
                HasPassword = !string.IsNullOrEmpty(u.PasswordHash)
            });
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            
            if (user == null)
            {
                return null;
            }

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                HasGoogleAuth = !string.IsNullOrEmpty(user.GoogleId),
                HasPassword = !string.IsNullOrEmpty(user.PasswordHash)
            };
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                _logger.LogWarning("Attempted to update status for non-existent user: {UserId}", userId);
                return false;
            }

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _userRepository.UpdateAsync(user);
            
            _logger.LogInformation("User status updated: UserId={UserId}, IsActive={IsActive}, Email={Email}", 
                userId, isActive, user.Email);
            
            return true;
        }
    }
}

