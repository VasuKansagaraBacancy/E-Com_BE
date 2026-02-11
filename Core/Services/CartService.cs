using E_Commerce.Core.DTOs;
using E_Commerce.Core.Entities;
using E_Commerce.Core.Exceptions;
using E_Commerce.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Core.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<CartService> _logger;

        public CartService(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CartDto>> GetUserCartAsync(int userId)
        {
            var cartItems = await _cartRepository.GetByUserIdAsync(userId);
            
            return cartItems.Select(c => new CartDto
            {
                Id = c.Id,
                ProductId = c.ProductId,
                ProductName = c.Product?.Name ?? "",
                ProductPrice = c.Product?.Price ?? 0,
                ProductImageUrl = c.Product?.ImageUrl,
                Quantity = c.Quantity,
                SubTotal = (c.Product?.Price ?? 0) * c.Quantity,
                CreatedAt = c.CreatedAt
            });
        }

        public async Task<CartDto> AddToCartAsync(AddToCartDto addToCartDto, int userId)
        {
            // Verify product exists and is approved
            var product = await _productRepository.GetByIdAsync(addToCartDto.ProductId);
            if (product == null)
            {
                throw new ValidationException("Product not found.");
            }

            if (product.Status != "Approved")
            {
                throw new ValidationException("Product is not available for purchase.");
            }

            if (!product.IsActive)
            {
                throw new ValidationException("Product is not active.");
            }

            if (product.StockQuantity < addToCartDto.Quantity)
            {
                throw new ValidationException($"Insufficient stock. Available: {product.StockQuantity}");
            }

            // Check if item already exists in cart
            var existingCartItem = await _cartRepository.GetByUserAndProductAsync(userId, addToCartDto.ProductId);

            Cart cartItem;
            if (existingCartItem != null)
            {
                // Update quantity
                var newQuantity = existingCartItem.Quantity + addToCartDto.Quantity;
                
                if (product.StockQuantity < newQuantity)
                {
                    throw new ValidationException($"Insufficient stock. Available: {product.StockQuantity}, Requested: {newQuantity}");
                }

                existingCartItem.Quantity = newQuantity;
                existingCartItem.UpdatedAt = DateTime.UtcNow;
                cartItem = await _cartRepository.UpdateAsync(existingCartItem);
            }
            else
            {
                // Create new cart item
                cartItem = new Cart
                {
                    UserId = userId,
                    ProductId = addToCartDto.ProductId,
                    Quantity = addToCartDto.Quantity,
                    CreatedAt = DateTime.UtcNow
                };
                cartItem = await _cartRepository.CreateAsync(cartItem);
            }

            _logger.LogInformation("Product added to cart: UserId={UserId}, ProductId={ProductId}, Quantity={Quantity}", 
                userId, addToCartDto.ProductId, cartItem.Quantity);

            // Reload with includes
            var updatedCartItem = await _cartRepository.GetByIdAsync(cartItem.Id);
            return MapToDto(updatedCartItem!);
        }

        public async Task<CartDto?> UpdateCartItemAsync(int cartItemId, UpdateCartItemDto updateDto, int userId)
        {
            var cartItem = await _cartRepository.GetByIdAsync(cartItemId);
            if (cartItem == null)
            {
                return null;
            }

            // Verify ownership
            if (cartItem.UserId != userId)
            {
                throw new ValidationException("You can only update your own cart items.");
            }

            // Verify product stock
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
            if (product == null || product.StockQuantity < updateDto.Quantity)
            {
                throw new ValidationException($"Insufficient stock. Available: {product?.StockQuantity ?? 0}");
            }

            cartItem.Quantity = updateDto.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            cartItem = await _cartRepository.UpdateAsync(cartItem);

            var updatedCartItem = await _cartRepository.GetByIdAsync(cartItem.Id);
            return MapToDto(updatedCartItem!);
        }

        public async Task<bool> RemoveFromCartAsync(int cartItemId, int userId)
        {
            var cartItem = await _cartRepository.GetByIdAsync(cartItemId);
            if (cartItem == null)
            {
                return false;
            }

            // Verify ownership
            if (cartItem.UserId != userId)
            {
                throw new ValidationException("You can only remove your own cart items.");
            }

            var result = await _cartRepository.DeleteAsync(cartItemId);
            if (result)
            {
                _logger.LogInformation("Item removed from cart: UserId={UserId}, CartItemId={CartItemId}", 
                    userId, cartItemId);
            }
            return result;
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var result = await _cartRepository.ClearUserCartAsync(userId);
            if (result)
            {
                _logger.LogInformation("Cart cleared: UserId={UserId}", userId);
            }
            return result;
        }

        public async Task<decimal> GetCartTotalAsync(int userId)
        {
            var cartItems = await _cartRepository.GetByUserIdAsync(userId);
            return cartItems.Sum(c => (c.Product?.Price ?? 0) * c.Quantity);
        }

        private CartDto MapToDto(Cart cart)
        {
            return new CartDto
            {
                Id = cart.Id,
                ProductId = cart.ProductId,
                ProductName = cart.Product?.Name ?? "",
                ProductPrice = cart.Product?.Price ?? 0,
                ProductImageUrl = cart.Product?.ImageUrl,
                Quantity = cart.Quantity,
                SubTotal = (cart.Product?.Price ?? 0) * cart.Quantity,
                CreatedAt = cart.CreatedAt
            };
        }
    }
}

