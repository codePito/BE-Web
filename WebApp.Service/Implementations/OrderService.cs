using AutoMapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;
using WebApp.Model.Request;
using WebApp.Model.Response;
using WebApp.Repository.Interfaces;
using WebApp.Service.Interfaces;

namespace WebApp.Service.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repo;
        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepo;
        public OrderService(IOrderRepository repo, IMapper mapper, IProductRepository productRepo)
        {
            _repo = repo;
            _mapper = mapper;
            _productRepo = productRepo;
        }

        public async Task<OrderResponse> CreateOrderAsync(OrderRequest request, int userId)
        {
            if (request == null || !request.Items.Any()) throw new ArgumentException("Order must have items");

            var order = new Order
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.PaymentPending,
                Currency = request.Currency
            };

            decimal total = 0;
            foreach(var item in request.Items )
            {
                var product = _productRepo.GetByID(item.ProductId);
                if (product == null) throw new Exception($"Product id {item.ProductId} not found");
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity
                };
                order.Items.Add(orderItem);
                total += orderItem.Total;

            }
                order.TotalAmount = total;
                
                var saved = await _repo.AddAsync(order);
                return _mapper.Map<OrderResponse>(saved);
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _repo.GetByIdAsync(id);
            if (order == null) return false;
            await _repo.DeleteAsync(id);
            return true;
        }

        public async Task<OrderResponse?> GetByIdAsync(int id)
        {
            var order = await _repo.GetByIdAsync(id);
            return order == null ? null : _mapper.Map<OrderResponse>(order);
        }

        public async Task<IEnumerable<OrderResponse>> GetByUserIdAsync(int userId)
        {
            var list = await _repo.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<OrderResponse>>(list);
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, OrderStatus status)
        {
            var order = await _repo.GetByIdAsync(id);
            if (order == null) return false;
            order.Status = status;
            await _repo.UpdateAsync(order);
            return true;
        }
    }
}
