using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Core.DTOs
{
    public class CreateProductCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}

