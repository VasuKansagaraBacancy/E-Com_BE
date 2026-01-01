using E_Commerce.Core.Entities;

namespace E_Commerce.Core.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
    }
}


