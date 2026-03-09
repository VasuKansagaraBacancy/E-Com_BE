using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce.Core.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty; // Store product name at time of order

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Store price at time of order

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; } // Price * Quantity

        [MaxLength(50)]
        public string ReturnStatus { get; set; } = "None"; // None, Requested, Approved, Rejected

        public DateTime? ReturnRequestedAt { get; set; }

        public DateTime? ReturnResolvedAt { get; set; }

        [MaxLength(500)]
        public string? ReturnReason { get; set; }

        /// <summary>
        /// Refund status when return is approved: None, Initiated, Done, Refunded. Updated by Admin; customer can view.
        /// </summary>
        [MaxLength(50)]
        public string RefundStatus { get; set; } = "None";

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}



