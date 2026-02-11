using E_Commerce.Core.DTOs;
using E_Commerce.Core.Entities;
using E_Commerce.Core.Exceptions;
using E_Commerce.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Core.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(o => MapToDto(o));
        }

        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return orders.Select(o => MapToDto(o));
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int orderId, int userId, string userRole)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return null;
            }

            // Users can only view their own orders, Admin can view all
            if (userRole != "Admin" && order.UserId != userId)
            {
                throw new ValidationException("You can only view your own orders.");
            }

            return MapToDto(order);
        }

        public async Task<OrderDto> CreateOrderFromCartAsync(CreateOrderDto createOrderDto, int userId)
        {
            // Get user's cart items
            var cartItems = await _cartRepository.GetByUserIdAsync(userId);
            var cartItemsList = cartItems.ToList();

            if (!cartItemsList.Any())
            {
                throw new ValidationException("Your cart is empty. Cannot create order.");
            }

            // Validate all products are still available and approved
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var cartItem in cartItemsList)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                
                if (product == null || !product.IsActive)
                {
                    throw new ValidationException($"Product '{cartItem.Product?.Name}' is no longer available.");
                }

                if (product.Status != "Approved")
                {
                    throw new ValidationException($"Product '{product.Name}' is not approved for purchase.");
                }

                if (product.StockQuantity < cartItem.Quantity)
                {
                    throw new ValidationException($"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}, Requested: {cartItem.Quantity}");
                }

                var itemTotal = product.Price * cartItem.Quantity;
                totalAmount += itemTotal;

                // Create order item
                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = cartItem.Quantity,
                    SubTotal = itemTotal
                };

                orderItems.Add(orderItem);

                // Update product stock
                product.StockQuantity -= cartItem.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            // Create order
            var order = new Order
            {
                UserId = userId,
                Status = "Pending",
                TotalAmount = totalAmount,
                ShippingAddress = createOrderDto.ShippingAddress,
                ShippingCity = createOrderDto.ShippingCity,
                ShippingState = createOrderDto.ShippingState,
                ShippingZipCode = createOrderDto.ShippingZipCode,
                ShippingCountry = createOrderDto.ShippingCountry,
                Notes = createOrderDto.Notes,
                CreatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            order = await _orderRepository.CreateAsync(order);

            // Clear user's cart after successful order
            await _cartRepository.ClearUserCartAsync(userId);

            _logger.LogInformation("Order created: OrderId={OrderId}, UserId={UserId}, TotalAmount={TotalAmount}", 
                order.Id, userId, totalAmount);

            var createdOrder = await _orderRepository.GetByIdAsync(order.Id);
            return MapToDto(createdOrder!);
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status, int adminUserId)
        {
            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(status))
            {
                throw new ValidationException($"Invalid order status. Valid statuses: {string.Join(", ", validStatuses)}");
            }

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return false;
            }

            // Prevent changing status of cancelled or delivered orders
            if (order.Status == "Cancelled" && status != "Cancelled")
            {
                throw new ValidationException("Cannot change status of a cancelled order.");
            }

            if (order.Status == "Delivered" && status != "Delivered")
            {
                throw new ValidationException("Cannot change status of a delivered order.");
            }

            // If changing to Cancelled for the first time, restore product stock
            var isCancellingNow = status == "Cancelled" && order.Status != "Cancelled";
            if (isCancellingNow)
            {
                foreach (var item in order.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        await _productRepository.UpdateAsync(product);
                    }
                }
            }

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            _logger.LogInformation("Order status updated: OrderId={OrderId}, NewStatus={Status}, AdminUserId={AdminUserId}", 
                orderId, status, adminUserId);

            return true;
        }

        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                UserEmail = order.User?.Email ?? "",
                UserName = $"{order.User?.FirstName} {order.User?.LastName}",
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                ShippingCity = order.ShippingCity,
                ShippingState = order.ShippingState,
                ShippingZipCode = order.ShippingZipCode,
                ShippingCountry = order.ShippingCountry,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    SubTotal = oi.SubTotal
                }).ToList()
            };
        }
    }
}

