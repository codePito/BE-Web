using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApp.Model.Entities;
using WebApp.Repository.Interfaces;

namespace WebApp.Service.BackgroundServices
{

    public class OrderCleanupService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<OrderCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); 
        private readonly int _expiryMinutes = 30; 

        public OrderCleanupService(IServiceProvider services, ILogger<OrderCleanupService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderCleanupService started. Running every {Interval} minutes", _interval.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredOrdersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in OrderCleanupService");
                }

                // Chờ 5 phút trước khi chạy lại
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("OrderCleanupService stopped");
        }

        private async Task CleanupExpiredOrdersAsync()
        {
            using var scope = _services.CreateScope();
            var orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();

            try
            {
                // Tìm orders PaymentPending quá 30 phút
                var expiredOrders = await orderRepo.GetExpiredPendingOrdersAsync(_expiryMinutes);
                var ordersList = expiredOrders.ToList();

                if (!ordersList.Any())
                {
                    _logger.LogDebug("No expired orders found");
                    return;
                }

                _logger.LogInformation("Found {Count} expired orders to cleanup", ordersList.Count);

                foreach (var order in ordersList)
                {
                    try
                    {
                        // Hoàn lại stock cho từng item
                        foreach (var item in order.Items)
                        {
                            var product = productRepo.GetByID(item.ProductId);
                            if (product != null)
                            {
                                product.StockQuantity += item.Quantity;
                                product.IsAvailable = true;
                                productRepo.Update(product);

                                _logger.LogInformation(
                                    "Restored {Quantity} stock for product {ProductId} from expired order {OrderId}",
                                    item.Quantity, product.Id, order.Id);
                            }
                        }

                        // Update order status sang Failed
                        order.Status = OrderStatus.Failed;
                        await orderRepo.UpdateAsync(order);

                        _logger.LogInformation(
                            "Order {OrderId} marked as Failed. Created: {CreatedAt}, Expiry: {Expiry}",
                            order.Id, order.CreatedAt, order.PaymentExpiry);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error cleaning up order {OrderId}", order.Id);
                    }
                }

                // Save tất cả thay đổi product
                await productRepo.SaveChangesAsync();

                _logger.LogInformation("Successfully cleaned up {Count} expired orders", ordersList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CleanupExpiredOrdersAsync");
                throw;
            }
        }
    }
}
