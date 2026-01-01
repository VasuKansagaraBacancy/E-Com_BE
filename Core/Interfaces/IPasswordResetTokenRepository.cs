using E_Commerce.Core.Entities;

namespace E_Commerce.Core.Interfaces
{
    public interface IPasswordResetOtpRepository
    {
        Task<PasswordResetOtp?> GetByOtpAsync(string otp, string email);
        Task<PasswordResetOtp> CreateAsync(PasswordResetOtp otpEntity);
        Task MarkAsUsedAsync(int otpId);
        Task InvalidateUserOtpsAsync(int userId);
    }
}


