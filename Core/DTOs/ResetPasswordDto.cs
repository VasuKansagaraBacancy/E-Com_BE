using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Core.DTOs
{
    public class ResetPasswordDto
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be a 6-digit number")]
        public string Otp { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public string NewPassword { get; set; } = string.Empty;
    }
}

