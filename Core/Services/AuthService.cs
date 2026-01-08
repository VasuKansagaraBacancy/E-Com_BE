using E_Commerce.Core.DTOs;
using E_Commerce.Core.Entities;
using E_Commerce.Core.Exceptions;
using E_Commerce.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

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
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IPasswordResetOtpRepository otpRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IEmailService emailService,
            ILogger<AuthService> logger,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _otpRepository = otpRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
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

        public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto googleLoginDto)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(googleLoginDto.IdToken))
                {
                    throw new AuthenticationException("Google ID token is required.");
                }

                // Log token info for debugging (first 20 chars only for security)
                var tokenPreview = googleLoginDto.IdToken.Length > 20 
                    ? googleLoginDto.IdToken.Substring(0, 20) + "..." 
                    : googleLoginDto.IdToken;
                _logger.LogInformation("Received Google token (length: {Length}, preview: {Preview})", 
                    googleLoginDto.IdToken.Length, tokenPreview);

                // Validate Google ID token
                var googleClientId = _configuration["GoogleOAuth:ClientId"];
                if (string.IsNullOrEmpty(googleClientId))
                {
                    _logger.LogError("Google OAuth ClientId is not configured in appsettings.json");
                    throw new AuthenticationException("Google OAuth is not configured on the server.");
                }

                _logger.LogInformation("Validating Google token with ClientId: {ClientId}", googleClientId);

                // Try to decode token to see what's in it (for debugging)
                try
                {
                    var parts = googleLoginDto.IdToken.Split('.');
                    if (parts.Length == 3)
                    {
                        // Decode the payload (second part)
                        var payloadPart = parts[1];
                        // Add padding if needed
                        while (payloadPart.Length % 4 != 0)
                        {
                            payloadPart += "=";
                        }
                        var payloadBytes = Convert.FromBase64String(payloadPart);
                        var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);
                        _logger.LogDebug("Token payload: {Payload}", payloadJson);
                    }
                }
                catch (Exception decodeEx)
                {
                    _logger.LogWarning(decodeEx, "Could not decode token for inspection");
                }

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { googleClientId }
                };

                GoogleJsonWebSignature.Payload? payload;
                try
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(googleLoginDto.IdToken, settings);
                }
                catch (InvalidJwtException ex)
                {
                    _logger.LogError(ex, "Invalid Google ID token. Error: {Error}, InnerException: {InnerException}, StackTrace: {StackTrace}", 
                        ex.Message, ex.InnerException?.Message, ex.StackTrace);
                    
                    // Provide more helpful error message
                    var errorMessage = "Invalid Google authentication token.";
                    if (ex.Message.Contains("audience") || ex.Message.Contains("aud"))
                    {
                        errorMessage += " The token's audience (Client ID) does not match the configured Client ID. Please verify that the Client ID in your frontend matches the one in appsettings.json.";
                    }
                    else if (ex.Message.Contains("expired") || ex.Message.Contains("exp"))
                    {
                        errorMessage += " The token has expired. Please sign in again.";
                    }
                    else if (ex.Message.Contains("signature"))
                    {
                        errorMessage += " Token signature validation failed. This might indicate a token from a different Google project.";
                    }
                    else
                    {
                        errorMessage += $" Details: {ex.Message}";
                    }
                    
                    throw new AuthenticationException(errorMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating Google token. Exception type: {Type}, Message: {Message}, StackTrace: {StackTrace}", 
                        ex.GetType().Name, ex.Message, ex.StackTrace);
                    throw new AuthenticationException($"Error validating Google token: {ex.Message}");
                }

                if (payload == null)
                {
                    throw new AuthenticationException("Failed to validate Google token.");
                }

                // Extract user information from Google token
                var email = payload.Email?.ToLowerInvariant() ?? throw new AuthenticationException("Email not provided by Google.");
                var googleId = payload.Subject;
                var firstName = payload.GivenName ?? "User";
                var lastName = payload.FamilyName ?? "";

                // Check if user exists by Google ID
                var user = await _userRepository.GetByGoogleIdAsync(googleId);

                if (user == null)
                {
                    // Check if user exists by email (might be existing user trying Google login)
                    user = await _userRepository.GetByEmailAsync(email);
                    
                    if (user != null)
                    {
                        // Existing user - link Google account
                        user.GoogleId = googleId;
                        user.UpdatedAt = DateTime.UtcNow;
                        user = await _userRepository.UpdateAsync(user);
                    }
                    else
                    {
                        // New user - create account with Customer role
                        user = new User
                        {
                            Email = email,
                            GoogleId = googleId,
                            FirstName = firstName,
                            LastName = lastName,
                            Role = "Customer", // Default role for Google sign-in users
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        user = await _userRepository.CreateAsync(user);
                        _logger.LogInformation("New user created via Google Sign-In: {Email}", email);
                    }
                }

                if (!user.IsActive)
                {
                    throw new AuthenticationException("Your account is inactive. Please contact support.");
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
            catch (AuthenticationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google login");
                throw new AuthenticationException("An error occurred during Google authentication.");
            }
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

            // Check if user has a password (not a Google-only user)
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                throw new AuthenticationException("This account was created with Google Sign-In. Please use Google Sign-In to login.");
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


