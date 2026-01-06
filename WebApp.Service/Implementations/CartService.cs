using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;
using WebApp.Model.Response;
using WebApp.Repository.Interfaces;
using WebApp.Service.Interfaces;

namespace WebApp.Service.Implementations
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _repo;
        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepo;

        public CartService(ICartRepository repo, IMapper mapper, IProductRepository productRepo)
        {
            _repo = repo;
            _mapper = mapper;
            _productRepo = productRepo;
        }

        public async Task<CartResponse> AddToCartAsync(int userId, int productId, int quantity)
        {
            var product = _productRepo.GetByID(productId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            if (!product.IsAvailable)
            {
                throw new Exception("Product is not available");
            }

            if (product.StockQuantity < quantity)
            {
                throw new Exception($"Not enough stock, only {product.StockQuantity} items available");
            }

            var cart = await GetOrCreateCart(userId);

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + quantity;

                if (product.StockQuantity < newQuantity)
                {
                    throw new Exception($"Cannot add {quantity} more items");
                }

                existingItem.Quantity = newQuantity;

                await _repo.UpdateItemAsync(existingItem);
            }
            else
            {
                var item = new CartItem
                {
                    CartId = cart.Id,
                    Quantity = quantity,
                    ProductId = productId,
                };
                await _repo.AddItemAsync(item);
            }

            await _repo.SaveChangesAsync();

            return await GetUserCartAsync(userId);
        }

        public async Task ClearCartAsync(int userId)
        {
            await _repo.ClearCartAsync(userId);
            await _repo.SaveChangesAsync();
        }

        public async Task<CartResponse> GetUserCartAsync(int userId)
        {
            var cart = await _repo.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _repo.AddCartAsync(cart);
                await _repo.SaveChangesAsync();
            }

            var response = _mapper.Map<CartResponse>(cart);

            foreach (var item in response.Items)
            {
                var cartItem = cart.Items.FirstOrDefault(i => i.Id == item.Id);
                if (cartItem != null && !string.IsNullOrEmpty(cartItem.VariantId))
                {
                    var product = _productRepo.GetByID(cartItem.ProductId);
                    if (product?.VariantsData?.HasVariants == true)
                    {
                        var variant = product.VariantsData.Options.FirstOrDefault(v => v.Id == cartItem.VariantId);
                        if (variant != null)
                        {
                            item.Price = product.Price + variant.PriceAdjustment;

                            if (string.IsNullOrEmpty(item.VariantInfo))
                            {
                                var parts = new List<string>();
                                if (!string.IsNullOrEmpty(variant.Color)) parts.Add($"Color: {variant.Color}");
                                if (!string.IsNullOrEmpty(variant.Size)) parts.Add($"Size: {variant.Size}");
                                item.VariantInfo = string.Join(", ", parts);

                                if (string.IsNullOrEmpty(cartItem.VariantInfo))
                                {
                                    cartItem.VariantInfo = item.VariantInfo;
                                    await _repo.UpdateItemAsync(cartItem);
                                    await _repo.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }
            }
            return response;
        }

        public async Task RemoveItemAsync(int userId, int itemId)
        {
            var cart = await _repo.GetCartByUserIdAsync(userId);

            var item = cart.Items.FirstOrDefault(i =>  i.Id == itemId);
            if (item == null) return;

            await _repo.RemoveItemAsync(item);
            await _repo.SaveChangesAsync();
        }

        public async Task<CartResponse> UpdateQuantityAsync(int userId, int quantity, int itemId)
        {
            var cart = await _repo.GetCartByUserIdAsync(userId);

            var item = cart.Items.FirstOrDefault(i =>  i.Id == itemId);

            if(item == null)
            {
                throw new Exception("Item not found");
            }

            var product = _productRepo.GetByID(item.ProductId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            if(!product.IsAvailable)
            {
                throw new Exception("Product is not available");
            }

            if (product.StockQuantity < quantity)
                throw new Exception($"Not enough stock, only {product.StockQuantity} items available");

            item.Quantity = quantity;

            await _repo.UpdateItemAsync(item);
            await _repo.SaveChangesAsync();

            return await GetUserCartAsync(userId);
        }
        private async Task<Cart> GetOrCreateCart(int userId)
        {
            var cart = await _repo.GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _repo.AddCartAsync(cart);
                await _repo.SaveChangesAsync();
            }

            return cart;
        }
        public async Task<CartResponse> AddToCartWithVariantAsync(int userId, int productId, int quantity, string? variantId = null)
        {
            var product = _productRepo.GetByID(productId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            if (!product.IsAvailable)
            {
                throw new Exception("Product is not available");
            }

            // Check variant stock if variantId provided
            string? variantInfo = null;
            if (!string.IsNullOrEmpty(variantId))
            {
                var variants = product.VariantsData;
                if (variants?.HasVariants == true)
                {
                    var variant = variants.Options.FirstOrDefault(v => v.Id == variantId);
                    if (variant == null)
                        throw new Exception($"Variant {variantId} not found");

                    if (variant.Stock < quantity)
                        throw new Exception($"Not enough stock for this variant, only {variant.Stock} available");

                    // Build variant info string
                    var parts = new List<string>();
                    if (!string.IsNullOrEmpty(variant.Color)) parts.Add($"Color: {variant.Color}");
                    if (!string.IsNullOrEmpty(variant.Size)) parts.Add($"Size: {variant.Size}");
                    variantInfo = string.Join(", ", parts);
                }
            }
            else
            {
                // No variant - check product stock
                if (product.StockQuantity < quantity)
                {
                    throw new Exception($"Not enough stock, only {product.StockQuantity} items available");
                }
            }

            var cart = await GetOrCreateCart(userId);

            // Check if item with same product and variant exists
            var existingItem = cart.Items.FirstOrDefault(i =>
                i.ProductId == productId && i.VariantId == variantId);

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + quantity;

                // Check stock again
                if (!string.IsNullOrEmpty(variantId))
                {
                    var variants = product.VariantsData;
                    var variant = variants?.Options.FirstOrDefault(v => v.Id == variantId);
                    if (variant != null && variant.Stock < newQuantity)
                    {
                        throw new Exception($"Cannot add {quantity} more items");
                    }
                }
                else
                {
                    if (product.StockQuantity < newQuantity)
                    {
                        throw new Exception($"Cannot add {quantity} more items");
                    }
                }

                existingItem.Quantity = newQuantity;

                if (string.IsNullOrEmpty(existingItem.VariantInfo) && !string.IsNullOrEmpty(variantInfo))
                {
                    existingItem.VariantInfo = variantInfo;
                }

                await _repo.UpdateItemAsync(existingItem);
            }
            else
            {
                var item = new CartItem
                {
                    CartId = cart.Id,
                    Quantity = quantity,
                    ProductId = productId,
                    VariantId = variantId,
                    VariantInfo = variantInfo
                };
                await _repo.AddItemAsync(item);
            }

            await _repo.SaveChangesAsync();

            return await GetUserCartAsync(userId);
        }
    }
}
