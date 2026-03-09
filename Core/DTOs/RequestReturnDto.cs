using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Core.DTOs
{
    public class RequestReturnDto
    {
        [Required]
        public int OrderItemId { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
