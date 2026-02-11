using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Core.DTOs
{
    public class CreateOrderDto
    {
        [MaxLength(200)]
        public string? ShippingAddress { get; set; }

        [MaxLength(100)]
        public string? ShippingCity { get; set; }

        [MaxLength(50)]
        public string? ShippingState { get; set; }

        [MaxLength(20)]
        public string? ShippingZipCode { get; set; }

        [MaxLength(100)]
        public string? ShippingCountry { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

