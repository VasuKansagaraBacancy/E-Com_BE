namespace E_Commerce.Core.DTOs
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
        public string ReturnStatus { get; set; } = "None"; // None, Requested, Approved, Rejected
        public DateTime? ReturnRequestedAt { get; set; }
        public DateTime? ReturnResolvedAt { get; set; }
        public string? ReturnReason { get; set; }
        /// <summary>Refund status: None, Initiated, Done, Refunded. Visible to customer in order details.</summary>
        public string RefundStatus { get; set; } = "None";
    }
}



