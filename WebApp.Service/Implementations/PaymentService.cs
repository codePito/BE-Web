using AutoMapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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
        private readonly IPaymentRepository _repo;
        private readonly IMomoService _momoService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        public PaymentService(IOrderRepository orderRepo, IPaymentRepository repo, IMomoService momoService, IMapper mapper, IConfiguration config)
        {
            _orderRepo = orderRepo;
            _repo = repo;
            _momoService = momoService;
            _mapper = mapper;
            _config = config;
        }

        public async Task<MomoPaymentResponse> CreateMomoPaymentAsync(MomoPaymentRequest request, int userId)
        {
            var order = await _orderRepo.GetByIdAsync(request.OrderId);
            if (order == null) throw new Exception("Order not found");
            if (order.UserId != userId) throw new UnauthorizedAccessException();
            if (order.Status == OrderStatus.Paid) throw new Exception("Order already paid");

            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                Currency = order.Currency,
                Provider = "Momo",
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            };
            var savedPayment = await _repo.AddAsync(payment);

            var returnUrl = request.ReturnUrl;
            var notifyUrl = request.NotifyUrl;
            var momoResponse = await _momoService.CreatePaymentAsync(order.Id, order.TotalAmount, returnUrl, notifyUrl);

            savedPayment.ProviderPaymentId = momoResponse.RequestId;
            savedPayment.RawResponse = JsonSerializer.Serialize(momoResponse);
            await _repo.UpdateAsync(savedPayment);

            return momoResponse;
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
            if (!_momoService.ValidateMomoSignature(requestBody, signatureHeader)) throw new Exception("Invalid signature");

            using var doc = JsonDocument.Parse(requestBody);
            var root = doc.RootElement;

            //momo test/prod body vary; adapt fields accordingly
            var orderIdStr = root.GetProperty("orderId").GetString();
            var requestId = root.GetProperty("requestId").GetString();
            var resultCode = root.GetProperty("resultCode").GetInt32(); //0 Success

            var amountStr = root.TryGetProperty("amount", out var a) ? a.GetString() ?? a.GetRawText() : "0";

            int orderId = int.Parse(orderIdStr!);
            var payment = await _repo.GetByOrderIdAsync(orderId);
            if (payment == null) throw new Exception("Payment not found");

            payment.ProviderPaymentId = requestId ?? payment.ProviderPaymentId;
            payment.RawResponse = requestBody;

            if (resultCode == 0)
            {
                payment.Status = PaymentStatus.Success;
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order != null) { }
                {
                    order.Status = OrderStatus.Paid;
                    await _orderRepo.UpdateAsync(order);
                }
                await _repo.UpdateAsync(payment);
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                await _repo.UpdateAsync(payment);
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
    }
}
