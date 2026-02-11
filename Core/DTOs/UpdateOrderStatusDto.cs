using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Core.DTOs
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty; // Pending, Processing, Shipped, Delivered, Cancelled
    }
}

