using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Core.DTOs
{
    public class ApproveProductDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public bool Approved { get; set; } // true = approve, false = reject
    }
}

