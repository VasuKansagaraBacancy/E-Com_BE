namespace E_Commerce.Core.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

