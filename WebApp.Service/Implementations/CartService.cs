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

        public CartService(ICartRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<CartResponse> AddToCartAsync(int userId, int productId, int quantity)
        {
            var cart = await GetOrCreateCart(userId);

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
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

            cart = await _repo.GetCartByUserIdAsync(userId);
            return _mapper.Map<CartResponse>(cart);
        }

        public async Task<CartResponse> GetUserCartAsync(int userId)
        {
            var cart = await _repo.GetCartByUserIdAsync(userId);
            if(cart == null)
            {
                cart = new Cart { UserId = userId };
                await _repo.AddCartAsync(cart);
                await _repo.SaveChangesAsync();
            }

            return _mapper.Map<CartResponse>(cart);
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
            item.Quantity = quantity;

            await _repo.UpdateItemAsync(item);
            await _repo.SaveChangesAsync();

            return _mapper.Map<CartResponse>(cart);
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
    }
}
