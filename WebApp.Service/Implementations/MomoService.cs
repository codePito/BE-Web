using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApp.Model.Response;
using WebApp.Service.Interfaces;
using static System.Net.WebRequestMethods;
using WebApp.Helper;
using Microsoft.Extensions.Logging;

namespace WebApp.Service.Implementations
{
    public class MomoService : IMomoService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<MomoService> _logger;

        // momo config
        private readonly string _partnerCode;
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _endpoint;
        private readonly string _requestType;
        private readonly string _isTestMode;
        public MomoService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
            _partnerCode = _config["Momo:PartnerCode"];
            _accessKey = _config["Momo:AccessKey"];
            _secretKey = _config["Momo:SecretKey"];
            _endpoint = _config["Momo:Endpoint"];
            _requestType = _config["Momo:RequestType"] ?? "captureWallet";

            _http.Timeout = TimeSpan.FromMilliseconds(
                int.Parse(_config["Momo:Timeout"] ?? "30000")
            );
        }

        public async Task<MomoPaymentResponse> CreatePaymentAsync(int orderId, decimal amount, string returnUrl, string notifyUrl)
        {
            var requestId = Guid.NewGuid().ToString();
            var orderIdStr = orderId.ToString();
            var orderInfo = $"Payment for order {orderId}";
            var amountStr = ((long)amount).ToString();
            var extraData = "";

            //raw signature string
            var rawSignature = $"accessKey={_accessKey}&amount={amountStr}&extraData={extraData}&ipnUrl={notifyUrl}&orderId={orderIdStr}&orderInfo={orderInfo}&partnerCode={_partnerCode}&returntUrl={returnUrl}&requestId={requestId}&requestType=captureWallet";

            //compute HMAC SHA256
            var signature = Security.ComputeHmacSha256(_secretKey, rawSignature);

            var payload = new
            {
                partnerCode = _partnerCode,
                accessKey = _accessKey,
                requestId = requestId,
                amount = amountStr,
                orderId = orderIdStr,
                orderInfo = orderInfo,
                redirectUrl = returnUrl,
                ipnUrl = notifyUrl,
                extraData = extraData,
                requestType = "captureWallet",
                signature = signature
            };

            var json = JsonSerializer.Serialize(payload);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync(_endpoint, httpContent);
            var respBody = await resp.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(respBody);
            var root = doc.RootElement;

            var resultCode = root.GetProperty("resultCode").GetInt32();
            var message = root.GetProperty("message").GetString();
            if(resultCode != 0)
            {
                throw new Exception($"Momo error: {message}");
            }
            var payUrl = root.GetProperty("payUrl").GetString();
            var momoRequestId = root.GetProperty("requestId").GetString();

            return new MomoPaymentResponse
            {
                OrderId = orderId,
                PayUrl = payUrl ?? string.Empty,
                RequestId = momoRequestId ?? string.Empty,
                Message = message ?? string.Empty
            };
        }

        public bool ValidateMomoSignature(string rawBody, string signature)
        {
            var computed = Security.ComputeHmacSha256(_secretKey, rawBody);
            return string.Equals(computed, signature, StringComparison.OrdinalIgnoreCase);
        }
    }
}
