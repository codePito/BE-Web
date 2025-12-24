using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
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

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var order = new Order
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Status = OrderStatus.PaymentPending,
                    Currency = request.Currency,
                    ShippingAddress = request.ShippingAddress,
                    PaymentExpiry = DateTime.UtcNow.AddMinutes(30)
                };

                decimal total = 0;


                foreach (var item in request.Items)
                {
                    var product = _productRepo.GetByID(item.ProductId);
                    if (product == null) throw new Exception($"Product id {item.ProductId} not found");

                    if (!product.IsAvailable)
                    {
                        throw new Exception($"Product id {item.ProductId} is not available");
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        throw new Exception($"Not enough stock for product id {item.ProductId}, only {product.StockQuantity} items available");
                    }

                    product.StockQuantity -= item.Quantity;

                    if (product.StockQuantity == 0)
                    {
                        product.IsAvailable = false;
                        _logger.LogWarning("Product {ProductId} '{ProductName}' is now out of stock", product.Id, product.Name);
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

                scope.Complete();

                _logger.LogInformation("Order {OrderId} created successfully. Total: {Total}, Expiry: {Expiry}",
                    saved.Id, saved.TotalAmount, saved.PaymentExpiry);

                return _mapper.Map<OrderResponse>(saved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for user {UserId}", userId);

                throw;
            }
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

        public async Task<IEnumerable<OrderResponse>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<OrderResponse>>(list);
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

        public async Task<IEnumerable<TopCustomerStatsResponse>> GetTopCustomersAsync(int topCount = 10)
        {
            var orders = await _repo.GetAllAsync();

            var completedOrders = orders.Where(o => o.Status == OrderStatus.Paid);

            var topCustomers = completedOrders
                .GroupBy(o => new
                {
                    o.UserId,
                    o.User.UserName,
                    o.User.Email
                })
                .Select(g => new TopCustomerStatsResponse
                {
                    UserId = g.Key.UserId,
                    UserName = g.Key.UserName,
                    Email = g.Key.Email,
                    TotalSpent = g.Sum(o => o.TotalAmount),
                    TotalOrders = g.Count()
                })
                .OrderByDescending(c => c.TotalOrders)
                .Take(topCount)
                .ToList();

            _logger.LogInformation("Retrieved top {Count} customers", topCustomers.Count);

            return topCustomers;
        }

        public async Task<IEnumerable<ProductSalesMonthlyStatsResponse>> GetProductSalesMonthlyAsync(int year)
        {
            var orders = await _repo.GetAllAsync();

            var completedOrders = orders.Where(o => o.Status == OrderStatus.Paid && o.CreatedAt.Year == year);

            var monthlyStats = completedOrders
                .GroupBy(o => o.CreatedAt.Month)
                .Select(g => new ProductSalesMonthlyStatsResponse
                {
                    Month = new DateTime(year, g.Key, 1).ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture),
                    TotalProducts = g.Sum(o => o.Items.Sum(i => i.Quantity)),
                    TotalOrders = g.Count(),
                    Year = year
                })
                .OrderBy(s => DateTime.ParseExact(s.Month, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month)
                .ToList();

            var allMonths = Enumerable.Range(1, 12)
                .Select(m => new ProductSalesMonthlyStatsResponse
                {
                    Month = new DateTime(year, m, 1).ToString("MMM", System.Globalization.CultureInfo.InvariantCulture),
                    TotalProducts = 0,
                    TotalOrders = 0,
                    Year = year
                })
                .ToList();

            foreach (var stat in monthlyStats)
            {
                var month = allMonths.FirstOrDefault(m => m.Month == stat.Month);
                if (month != null)
                {
                    month.TotalProducts = stat.TotalProducts;
                    month.TotalOrders = stat.TotalOrders;
                }
            }

            _logger.LogInformation("Retrieved product sales stats for year {Year}", year);

            return allMonths;
        }
    }
}
