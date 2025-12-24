using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Transactions;
using WebApp.Model.Entities;
using WebApp.Model.Request;
using WebApp.Model.Response;
using WebApp.Repository.Interfaces;
using WebApp.Service.Interfaces;

namespace WebApp.Service.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ICartRepository _cartRepo;
        private readonly IProductRepository _productRepo;
        private readonly IPaymentRepository _repo;
        private readonly IMomoService _momoService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentService> _logger;
        public PaymentService(IOrderRepository orderRepo, IPaymentRepository repo, IMomoService momoService, IMapper mapper, IConfiguration config, ILogger<PaymentService> logger, ICartRepository cartRepo, IProductRepository productRepo)
        {
            _orderRepo = orderRepo;
            _repo = repo;
            _momoService = momoService;
            _mapper = mapper;
            _config = config;
            _logger = logger;
            _cartRepo = cartRepo;
            _productRepo = productRepo;
        }

        public async Task<MomoPaymentResponse> CreateMomoPaymentAsync(MomoPaymentRequest request, int userId)
        {
            try
            {
                var order = await _orderRepo.GetByIdAsync(request.OrderId);
                if (order == null) throw new Exception("Order not found");
                if (order.UserId != userId) throw new UnauthorizedAccessException("You don't have permission to pay this order");
                if (order.Status == OrderStatus.Paid) throw new Exception("Order already paid");

                var payment = new Payment
                {
                    OrderId = order.Id,
                    Amount = order.TotalAmount,
                    Currency = order.Currency,
                    Provider = "MoMo",
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                };
                var savedPayment = await _repo.AddAsync(payment);

                var returnUrl = string.IsNullOrEmpty(request.ReturnUrl)
                    ? _config["Momo:ReturnUrl"] ?? "http://localhost:5173/"
                    : request.ReturnUrl;

                var notifyUrl = string.IsNullOrEmpty(request.NotifyUrl)
                    ? _config["Momo:NotifyUrl"] ?? "https://google.com"
                    : request.NotifyUrl;

                var momoResponse = await _momoService.CreatePaymentAsync(
                    order.Id,
                    order.TotalAmount,
                    returnUrl,
                    notifyUrl
                );

                // Update payment with MoMo response
                savedPayment.ProviderPaymentId = momoResponse.RequestId;
                savedPayment.RawResponse = JsonSerializer.Serialize(momoResponse);
                await _repo.UpdateAsync(savedPayment);

                return momoResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MoMo payment");

                var payment = await _repo.GetByOrderIdAsync(request.OrderId);
                if (payment != null)
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.RawResponse = ex.Message;
                    await _repo.UpdateAsync(payment);
                }

                throw;
            }
        }

        public async Task<PaymentResponse?> GetPaymentByIdAsync(int id)
        {
            var result = await _repo.GetByIdAsync(id);
            return result == null ? null : _mapper.Map<PaymentResponse>(result);
        }

        public async Task<IEnumerable<PaymentResponse>> GetPaymentsByOrderIdAsync(int orderId)
        {
            var list = await _repo.GetByOrderIdAllAsync(orderId);
            return list.Select(p => _mapper.Map<PaymentResponse>(p)).ToList();
        }

        public async Task HandleMomoNotifyAsync(string requestBody, string signatureHeader)
        {
            _logger.LogInformation("=== HandleMomoNotifyAsync START ===");

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                if (!_momoService.ValidateMomoSignature(requestBody, signatureHeader))
                {
                    _logger.LogError("Invalid MoMo signature!");
                    throw new Exception("Invalid signature");
                }

                using var doc = JsonDocument.Parse(requestBody);
                var root = doc.RootElement;

                var momoOrderIdStr = root.GetProperty("orderId").GetString();
                var requestId = root.GetProperty("requestId").GetString();
                var resultCode = root.GetProperty("resultCode").GetInt32();

                _logger.LogInformation("MoMo IPN: orderId={OrderId}, requestId={RequestId}, resultCode={ResultCode}",
                    momoOrderIdStr, requestId, resultCode);

                var orderId = int.Parse(momoOrderIdStr?.Split('_')[0] ?? "0");

                _logger.LogInformation("Parsed orderId: {OrderId}", orderId);

                var payment = await _repo.GetByOrderIdAsync(orderId);
                if (payment == null)
                {
                    _logger.LogError("Payment not found for order {OrderId}", orderId);
                    throw new Exception("Payment not found");
                }

                if (payment.Status == PaymentStatus.Success || payment.Status == PaymentStatus.Failed)
                {
                    _logger.LogWarning("Payment {PaymentId} already processed with status {Status}. Skipping.",
                        payment.Id, payment.Status);
                    scope.Complete(); 
                    return;
                }

                payment.ProviderPaymentId = requestId ?? payment.ProviderPaymentId;
                payment.RawResponse = requestBody;

                if (resultCode == 0)
                {
                    payment.Status = PaymentStatus.Success;
                    var order = await _orderRepo.GetByIdAsync(orderId);
                    if (order != null)
                    {
                        if (order.Status == OrderStatus.Paid)
                        {
                            _logger.LogWarning("Order {OrderId} already marked as Paid. Skipping.", orderId);
                            scope.Complete();
                            return;
                        }

                        order.Status = OrderStatus.Paid;
                        await _orderRepo.UpdateAsync(order);

                        foreach (var item in order.Items)
                        {
                            var product = _productRepo.GetByID(item.ProductId);
                            if (product != null)
                            {
                                product.SoldCount += item.Quantity;
                                _productRepo.Update(product);
                                _logger.LogInformation("Updated SoldCount for product {ProductId}: +{Quantity} (Total: {SoldCount})",
                                    product.Id, item.Quantity, product.SoldCount);
                            }
                        }

                        await _productRepo.SaveChangesAsync();

                        await _cartRepo.ClearCartAsync(order.UserId);
                        await _cartRepo.SaveChangesAsync();

                        _logger.LogInformation("✅ Order {OrderId} paid successfully. Cart cleared for user {UserId}",
                            orderId, order.UserId);
                    }
                }
                else
                {
                    payment.Status = PaymentStatus.Failed;
                    _logger.LogWarning("❌ Payment failed for order {OrderId}. ResultCode: {ResultCode}",
                        orderId, resultCode);
                }

                await _repo.UpdateAsync(payment);

                scope.Complete();

                _logger.LogInformation("=== HandleMomoNotifyAsync END (Success) ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== HandleMomoNotifyAsync END (Error) ===");
                
                throw;
            }
        }

        public async Task<PaymentResponse> RetryPaymentAsync(int paymentId, int userId)
        {
            var existing = await _repo.GetByIdAsync(paymentId);
            if (existing == null) throw new Exception("Payment not found");

            var order = await _orderRepo.GetByIdAsync(existing.OrderId);
            if (order == null) throw new Exception("Order not found");
            if (order.UserId != userId) throw new UnauthorizedAccessException();

            // create new payment record as retry
            var newPayment = new Payment
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                Currency = order.Currency,
                Provider = existing.Provider,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            var saved = await _repo.AddAsync(newPayment);

            // call momo with new attempt
            var returnUrl = _config["Frontend:Url"] + "/payment-result"; // or pass in
            var notifyUrl = _config["Backend:NotifyUrl"] ?? _config["Momo:NotifyUrl"];
            var momoResp = await _momoService.CreatePaymentAsync(order.Id, order.TotalAmount, returnUrl, notifyUrl);

            saved.ProviderPaymentId = momoResp.RequestId;
            saved.RawResponse = JsonSerializer.Serialize(momoResp);
            await _repo.UpdateAsync(saved);

            return _mapper.Map<PaymentResponse>(saved);
        }

        public async Task<bool> ConfirmPaymentAsync(int orderId, int resultCode, int userId)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("ConfirmPayment: Order {OrderId} not found", orderId);
                    return false;
                }

                if (order.UserId != userId)
                {
                    _logger.LogWarning("ConfirmPayment: User {UserId} không có quyền với order {OrderId}", userId, orderId);
                    return false;
                }

                if (order.Status != OrderStatus.PaymentPending)
                {
                    _logger.LogInformation("ConfirmPayment: Order {OrderId} đã có status {Status}, không cần cập nhật", orderId, order.Status);
                    return order.Status == OrderStatus.Paid;
                }

                var payment = await _repo.GetByOrderIdAsync(orderId);

                if (resultCode == 0)
                {
                    order.Status = OrderStatus.Paid;
                    if (payment != null)
                    {
                        payment.Status = PaymentStatus.Success;
                        await _repo.UpdateAsync(payment);
                    }

                    foreach (var item in order.Items)
                    {
                        var product = _productRepo.GetByID(item.ProductId);
                        if (product != null)
                        {
                            product.SoldCount += item.Quantity;
                            _productRepo.Update(product);
                            _logger.LogInformation("Updated SoldCount for product {ProductId}: +{Quantity} (Total: {SoldCount})",
                                product.Id, item.Quantity, product.SoldCount);
                        }
                    }
                    await _productRepo.SaveChangesAsync();

                    _logger.LogInformation("ConfirmPayment: Order {OrderId} đã được cập nhật thành Paid", orderId);
                }
                else
                {
                    order.Status = OrderStatus.Failed;
                    if (payment != null)
                    {
                        payment.Status = PaymentStatus.Failed;
                        await _repo.UpdateAsync(payment);
                    }

                    await _cartRepo.ClearCartAsync(order.UserId);
                    await _cartRepo.SaveChangesAsync();

                    _logger.LogInformation("ConfirmPayment: Order {OrderId} đã được cập nhật thành Paid. Cart cleared.", orderId);
                }

                await _orderRepo.UpdateAsync(order);

                scope.Complete();
                return resultCode == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment for order {OrderId}", orderId);
                throw;
            }

        }

    }
}
