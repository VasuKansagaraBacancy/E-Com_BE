using E_Commerce.Core.DTOs;
using E_Commerce.Core.Exceptions;
using E_Commerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var result = await _authService.RegisterAsync(registerDto);

                return Ok(new ApiResponseDto<AuthResponseDto>
                {
                    Success = true,
                    Message = "Registration successful",
                    Data = result
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new ApiResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "An error occurred during registration"
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var result = await _authService.LoginAsync(loginDto);

                return Ok(new ApiResponseDto<AuthResponseDto>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = result
                });
            }
            catch (AuthenticationException ex)
            {
                return Unauthorized(new ApiResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new ApiResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponseDto<object>>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                await _authService.ForgotPasswordAsync(forgotPasswordDto);

                // Always return success to prevent email enumeration
                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "If the email exists, a 6-digit OTP has been sent to your email."
                });
            }
            catch (AuthenticationException ex)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred processing your request"
                });
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponseDto<object>>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                await _authService.ResetPasswordAsync(resetPasswordDto);

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Password has been reset successfully"
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (AuthenticationException ex)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred during password reset"
                });
            }
        }
    }
}

