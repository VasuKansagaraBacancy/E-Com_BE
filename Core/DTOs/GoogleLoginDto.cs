using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Core.DTOs
{
    public class GoogleLoginDto
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}

