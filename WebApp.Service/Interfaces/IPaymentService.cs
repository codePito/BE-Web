using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;
using WebApp.Model.Request;
using WebApp.Model.Response;
using WebApp.Repository.Interfaces;

namespace WebApp.Service.Interfaces
{
    public interface IPaymentService
    {
        Task<MomoPaymentResponse> CreateMomoPaymentAsync(MomoPaymentRequest request, int userId);
        Task HandleMomoNotifyAsync(string requestBody, string signatureHeader);
        Task<IEnumerable<PaymentResponse>>  GetPaymentsByOrderIdAsync(int orderId);
        Task<PaymentResponse> GetPaymentByIdAsync(int id);
        Task<PaymentResponse> RetryPaymentAsync(int paymentId, int userId);

    }
}
