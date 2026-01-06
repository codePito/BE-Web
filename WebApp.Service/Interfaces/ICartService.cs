using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Response;

namespace WebApp.Service.Interfaces
{
    public interface ICartService
    {
        Task<CartResponse> GetUserCartAsync(int userId);
        Task<CartResponse> AddToCartAsync(int userId, int productId, int quantity);
        Task<CartResponse> AddToCartWithVariantAsync(int userId, int productId, int quantity, string? variantId = null);
        Task<CartResponse> UpdateQuantityAsync(int userId, int quantity, int itemId);
        Task ClearCartAsync(int userId);
        Task RemoveItemAsync(int userId, int itemId);
    }
}
