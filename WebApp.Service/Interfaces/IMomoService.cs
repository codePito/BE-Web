using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Request;
using WebApp.Model.Response;

namespace WebApp.Service.Interfaces
{
    public interface IMomoService
    {
        //Task<MomoPaymentResponse> CreatePaymentAsync(int orderId, decimal amount, string returnUrl, string notifyUrl);
        //bool ValidateMomoSignature(string rawBody, string signature);

        Task<MomoPaymentResponse> CreatePaymentAsync(int orderId, decimal amount, string returnUrl, string notifyUrl);

        /// <summary>
        /// Validate signature từ MoMo callback
        /// </summary>
        bool ValidateMomoSignature(string rawBody, string signature);

        /// <summary>
        /// Query trạng thái giao dịch từ MoMo (optional - để check status)
        /// </summary>
        Task<string> QueryTransactionStatusAsync(string orderId, string requestId);
    }
}
