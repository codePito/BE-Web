using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;
using WebApp.Model.Request;
using WebApp.Model.Response;

namespace WebApp.Service.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrderAsync(OrderRequest request, int userId);
        Task<OrderResponse> GetByIdAsync(int id);
        Task<IEnumerable<OrderResponse>> GetAllAsync();
        Task<IEnumerable<OrderResponse>> GetByUserIdAsync(int userId);
        Task<bool> DeleteOrderAsync(int id);
        Task<bool> UpdateOrderStatusAsync(int id, OrderStatus status);
        Task<IEnumerable<TopCustomerStatsResponse>> GetTopCustomersAsync(int topCount = 10);
        Task<IEnumerable<ProductSalesMonthlyStatsResponse>> GetProductSalesMonthlyAsync(int year);
    }
}
