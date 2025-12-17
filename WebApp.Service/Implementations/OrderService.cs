using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<OrderService> _logger;
        public OrderService(IOrderRepository repo, IMapper mapper, IProductRepository productRepo, ILogger<OrderService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _productRepo = productRepo;
            _logger = logger;
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

                if (!product.IsAvailable)
                {
                    throw new Exception($"Product id {item.ProductId} is not available");
                }

                if(product.StockQuantity < item.Quantity)
                {
                    throw new Exception($"Not enough stock for product id {item.ProductId}, only {product.StockQuantity} items available");
                }

                product.StockQuantity -= item.Quantity;

                if (product.StockQuantity == 0)
                {
                    product.IsAvailable = false;
                    _logger.LogWarning("Product {ProductId} '{ProductName}' is now out of stock",product.Id, product.Name);
                }

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
            await _productRepo.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} created successfully. Total: {Total}", saved.Id, saved.TotalAmount);

            return _mapper.Map<OrderResponse>(saved);
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _repo.GetByIdAsync(id);
            if (order == null) return false;

            if (order.Status == OrderStatus.Pending || order.Status == OrderStatus.PaymentPending)
            {
                foreach (var item in order.Items)
                {
                    var product = _productRepo.GetByID(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        product.IsAvailable = true;
                        _productRepo.Update(product);

                        _logger.LogInformation("Restored {Quantity} stock for product {ProducId}", item.Quantity, product.Id);
                    }
                }
                await _productRepo.SaveChangesAsync();
            }

            await _repo.DeleteAsync(id);
            _logger.LogInformation("Order {OrderId} deleted successfully", id);
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

            var oldStatus = order.Status;

            order.Status = status;

            if (status == OrderStatus.Cancelled &&
                (oldStatus == OrderStatus.Pending ||
                 oldStatus == OrderStatus.PaymentPending ||
                 oldStatus == OrderStatus.Paid))
            {
                foreach (var item in order.Items)
                {
                    var product = _productRepo.GetByID(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        product.IsAvailable = true;
                        _productRepo.Update(product);

                        _logger.LogInformation("Restored {Quantity} stock for product {ProductId} due to order cancellation",
                            item.Quantity, product.Id);
                    }
                }
                await _productRepo.SaveChangesAsync();
            }

            await _repo.UpdateAsync(order);

            _logger.LogInformation("Order {OrderId} status changed from {OldStatus} to {NewStatus}",
                order.Id, oldStatus, status);

            return true;
        }
    }
}
