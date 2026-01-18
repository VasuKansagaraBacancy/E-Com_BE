using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Core.DTOs
{
    public class UpdateUserStatusDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}

