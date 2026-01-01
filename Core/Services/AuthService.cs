using E_Commerce.Core.DTOs;
using E_Commerce.Core.Entities;
using E_Commerce.Core.Exceptions;
using E_Commerce.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetOtpRepository _otpRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IPasswordResetOtpRepository otpRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IEmailService emailService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _otpRepository = otpRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(registerDto.Email))
            {
                throw new ValidationException("Email address is already registered.");
            }

            // Validate role
            var validRoles = new[] { "Admin", "Seller", "Customer" };
            if (!validRoles.Contains(registerDto.Role))
            {
                registerDto.Role = "Customer";
            }

            // Hash password
            var passwordHash = _passwordHasher.HashPassword(registerDto.Password);

            // Create user
            var user = new User
            {
                Email = registerDto.Email.ToLowerInvariant(),
                PasswordHash = passwordHash,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Role = registerDto.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            user = await _userRepository.CreateAsync(user);

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email.ToLowerInvariant());

            if (user == null)
            {
                throw new AuthenticationException("Invalid email or password.");
            }

            if (!user.IsActive)
            {
                throw new AuthenticationException("Your account is inactive. Please contact support.");
            }

            if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                throw new AuthenticationException("Invalid email or password.");
            }

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        public async Task<ForgotPasswordResultDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var email = forgotPasswordDto.Email.ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(email);

            // CRITICAL: Security check - if user doesn't exist, return immediately
            // DO NOT generate OTP or send email for non-existent users
            if (user == null)
            {
                // User doesn't exist - return result indicating email doesn't exist
                _logger.LogInformation("Password reset requested for non-existent email: {Email}", email);
                return new ForgotPasswordResultDto
                {
                    EmailExists = false,
                    OtpSent = false
                };
            }

            // Additional safety check - ensure user is valid before proceeding
            if (user.Id <= 0)
            {
                // Invalid user - return result indicating email doesn't exist
                _logger.LogWarning("Password reset requested for user with invalid ID: {Email}", email);
                return new ForgotPasswordResultDto
                {
                    EmailExists = false,
                    OtpSent = false
                };
            }

            if (!user.IsActive)
            {
                throw new AuthenticationException("Your account is inactive. Please contact support.");
            }

            // Only proceed with OTP generation if user exists and is valid
            // Double-check user is not null before proceeding (defensive programming)
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                _logger.LogError("CRITICAL: User became null after validation check for email: {Email}", email);
                return new ForgotPasswordResultDto
                {
                    EmailExists = false,
                    OtpSent = false
                };
            }

            // Invalidate existing OTPs
            await _otpRepository.InvalidateUserOtpsAsync(user.Id);

            // Generate 6-digit OTP
            var otp = GenerateOtp();

            // Create password reset OTP (expires in 10 minutes)
            var otpEntity = new PasswordResetOtp
            {
                UserId = user.Id,
                Otp = otp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _otpRepository.CreateAsync(otpEntity);

            // FINAL SAFETY CHECK: Ensure user is still valid before sending email
            if (user == null || user.Id <= 0 || string.IsNullOrEmpty(user.Email))
            {
                _logger.LogError("CRITICAL: User validation failed right before email send for email: {Email}", email);
                return new ForgotPasswordResultDto
                {
                    EmailExists = false,
                    OtpSent = false
                };
            }

            // Send email with OTP - only called if user exists and is valid
            _logger.LogInformation("Sending password reset OTP to existing user: {Email}", user.Email);
            await _emailService.SendPasswordResetOtpAsync(user.Email, otp);

            return new ForgotPasswordResultDto
            {
                EmailExists = true,
                OtpSent = true
            };
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var email = resetPasswordDto.Email.ToLowerInvariant();
            var otpEntity = await _otpRepository.GetByOtpAsync(resetPasswordDto.Otp, email);

            if (otpEntity == null)
            {
                throw new ValidationException("Invalid OTP or email.");
            }

            if (otpEntity.IsUsed)
            {
                throw new ValidationException("This OTP has already been used.");
            }

            if (otpEntity.ExpiresAt < DateTime.UtcNow)
            {
                throw new ValidationException("This OTP has expired. Please request a new one.");
            }

            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null || user.Id != otpEntity.UserId)
            {
                throw new ValidationException("Invalid email or OTP.");
            }

            if (!user.IsActive)
            {
                throw new AuthenticationException("Your account is inactive. Please contact support.");
            }

            // Hash new password
            user.PasswordHash = _passwordHasher.HashPassword(resetPasswordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            // Mark OTP as used
            await _otpRepository.MarkAsUsedAsync(otpEntity.Id);

            return true;
        }

        private string GenerateOtp()
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomNumber = BitConverter.ToUInt32(bytes, 0);
            // Generate a 6-digit OTP
            var otp = (randomNumber % 900000 + 100000).ToString();
            return otp;
        }
    }
}


