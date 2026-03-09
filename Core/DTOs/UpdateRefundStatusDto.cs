using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Core.DTOs
{
    public class UpdateRefundStatusDto
    {
        [Required]
        public int OrderItemId { get; set; }

        /// <summary>Valid values: None, Initiated, Done, Refunded</summary>
        [Required]
        [MaxLength(50)]
        public string RefundStatus { get; set; } = string.Empty;
    }
}
