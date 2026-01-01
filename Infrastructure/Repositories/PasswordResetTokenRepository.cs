using E_Commerce.Core.Entities;
using E_Commerce.Core.Interfaces;
using E_Commerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Repositories
{
    public class PasswordResetOtpRepository : IPasswordResetOtpRepository
    {
        private readonly ApplicationDbContext _context;

        public PasswordResetOtpRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PasswordResetOtp?> GetByOtpAsync(string otp, string email)
        {
            return await _context.PasswordResetOtps
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Otp == otp && t.User.Email == email);
        }

        public async Task<PasswordResetOtp> CreateAsync(PasswordResetOtp otpEntity)
        {
            _context.PasswordResetOtps.Add(otpEntity);
            await _context.SaveChangesAsync();
            return otpEntity;
        }

        public async Task MarkAsUsedAsync(int otpId)
        {
            var otp = await _context.PasswordResetOtps.FindAsync(otpId);
            if (otp != null)
            {
                otp.IsUsed = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task InvalidateUserOtpsAsync(int userId)
        {
            var otps = await _context.PasswordResetOtps
                .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            foreach (var otp in otps)
            {
                otp.IsUsed = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}


