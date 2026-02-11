using E_Commerce.Core.DTOs;

namespace E_Commerce.Core.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId);
        Task<OrderDto?> GetOrderByIdAsync(int orderId, int userId, string userRole);
        Task<OrderDto> CreateOrderFromCartAsync(CreateOrderDto createOrderDto, int userId);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status, int adminUserId);
    }
}

