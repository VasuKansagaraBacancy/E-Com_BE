using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Core.DTOs
{
    public class ResolveReturnDto
    {
        [Required]
        public int OrderItemId { get; set; }

        [Required]
        public bool Approved { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
