using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Core.Entities
{
    public class PasswordResetOtp
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(6)]
        public string Otp { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}

