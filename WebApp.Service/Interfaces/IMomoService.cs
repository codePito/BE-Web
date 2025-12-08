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
        Task<MomoPaymentResponse> CreatePaymentAsync(int orderId, decimal amount, string returnUrl, string notifyUrl);
        bool ValidateMomoSignature(string rawBody, string signature);
    }
}
