namespace E_Commerce.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetOtpAsync(string email, string otp);
    }
}

