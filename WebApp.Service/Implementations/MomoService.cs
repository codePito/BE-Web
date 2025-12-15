using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using WebApp.Helper;
using WebApp.Model.Response;
using WebApp.Service.Interfaces;

namespace WebApp.Service.Implementations
{
    public class MomoService : IMomoService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<MomoService> _logger;

        // MoMo config
        private readonly string _partnerCode;
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _endpoint;
        private readonly string _requestType;

        public MomoService(HttpClient http, IConfiguration config, ILogger<MomoService> logger)
        {
            _http = http;
            _config = config;
            _logger = logger;

            _partnerCode = _config["Momo:PartnerCode"] ?? throw new ArgumentNullException("Momo:PartnerCode");
            _accessKey = _config["Momo:AccessKey"] ?? throw new ArgumentNullException("Momo:AccessKey");
            _secretKey = _config["Momo:SecretKey"] ?? throw new ArgumentNullException("Momo:SecretKey");
            _endpoint = _config["Momo:Endpoint"] ?? throw new ArgumentNullException("Momo:Endpoint");
            _requestType = _config["Momo:RequestType"] ?? "captureWallet";

            _http.Timeout = TimeSpan.FromMilliseconds(
                int.Parse(_config["Momo:Timeout"] ?? "30000")
            );
        }

        public async Task<MomoPaymentResponse> CreatePaymentAsync(int orderId, decimal amount, string returnUrl, string notifyUrl)
        {
            try
            {
                var requestId = Guid.NewGuid().ToString();

                // ✅ Thêm timestamp để tránh trùng orderId khi retry
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var orderIdStr = $"{orderId}_{timestamp}"; // VD: "2_1702345678"

                var orderInfo = $"Payment for order {orderId}";
                var amountStr = ((long)amount).ToString();
                var extraData = "";

                var orderExpireTime = 15;

                // Raw signature string - ĐÚNG thứ tự alphabet
                var rawSignature = $"accessKey={_accessKey}&amount={amountStr}&extraData={extraData}&ipnUrl={notifyUrl}&orderId={orderIdStr}&orderInfo={orderInfo}&partnerCode={_partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType={_requestType}";

                _logger.LogInformation("MoMo Raw Signature: {RawSignature}", rawSignature);

                var signature = Security.ComputeHmacSha256(rawSignature, _secretKey);

                _logger.LogInformation("MoMo Computed Signature: {Signature}", signature);

                var payload = new
                {
                    partnerCode = _partnerCode,
                    accessKey = _accessKey,
                    requestId = requestId,
                    amount = amountStr,
                    orderId = orderIdStr, // ✅ Dùng orderIdStr có timestamp
                    orderInfo = orderInfo,
                    redirectUrl = returnUrl,
                    ipnUrl = notifyUrl,
                    extraData = extraData,
                    requestType = _requestType,
                    signature = signature,
                    orderExpireTime = orderExpireTime,
                    lang = "vi"
                };

                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                _logger.LogInformation("MoMo Request Payload: {Payload}", json);

                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await _http.PostAsync(_endpoint, httpContent);
                var respBody = await resp.Content.ReadAsStringAsync();

                _logger.LogInformation("MoMo Response: {Response}", respBody);

                using var doc = JsonDocument.Parse(respBody);
                var root = doc.RootElement;

                var resultCode = root.GetProperty("resultCode").GetInt32();
                var message = root.GetProperty("message").GetString() ?? "Unknown error";

                if (resultCode != 0)
                {
                    _logger.LogError("MoMo Error - ResultCode: {ResultCode}, Message: {Message}", resultCode, message);
                    throw new Exception($"MoMo error: {message}. ResultCode: {resultCode}");
                }

                var payUrl = root.GetProperty("payUrl").GetString();
                var momoRequestId = root.GetProperty("requestId").GetString();

                return new MomoPaymentResponse
                {
                    OrderId = orderId,
                    PayUrl = payUrl ?? string.Empty,
                    RequestId = momoRequestId ?? string.Empty,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MoMo payment for order {OrderId}", orderId);
                throw;
            }
        }

        public bool ValidateMomoSignature(string rawBody, string signature)
        {
            try
            {
                if (string.IsNullOrEmpty(rawBody) || string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("Empty rawBody or signature for validation");
                    return false;
                }

                // Parse JSON để lấy các field cần thiết
                using var doc = JsonDocument.Parse(rawBody);
                var root = doc.RootElement;

                var partnerCode = root.GetProperty("partnerCode").GetString();
                var accessKey = root.GetProperty("accessKey").GetString();
                var requestId = root.GetProperty("requestId").GetString();
                var amount = root.GetProperty("amount").GetString();
                var orderId = root.GetProperty("orderId").GetString();
                var orderInfo = root.GetProperty("orderInfo").GetString();
                var orderType = root.TryGetProperty("orderType", out var ot) ? ot.GetString() : "";
                var transId = root.GetProperty("transId").GetString();
                var resultCode = root.GetProperty("resultCode").GetString();
                var message = root.GetProperty("message").GetString();
                var payType = root.TryGetProperty("payType", out var pt) ? pt.GetString() : "";
                var responseTime = root.GetProperty("responseTime").GetString();
                var extraData = root.TryGetProperty("extraData", out var ed) ? ed.GetString() : "";

                // Build raw signature theo format MoMo callback
                var rawSignature = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&message={message}&orderId={orderId}&orderInfo={orderInfo}&orderType={orderType}&partnerCode={partnerCode}&payType={payType}&requestId={requestId}&responseTime={responseTime}&resultCode={resultCode}&transId={transId}";

                _logger.LogInformation("Validate MoMo Raw Signature: {RawSignature}", rawSignature);

                var computed = Security.ComputeHmacSha256(rawSignature, _secretKey);

                _logger.LogInformation("Computed Signature: {Computed}, Received Signature: {Received}", computed, signature);

                return string.Equals(computed, signature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating MoMo signature");
                return false;
            }
        }

        public async Task<string> QueryTransactionStatusAsync(string orderId, string requestId)
        {
            try
            {
                var queryEndpoint = _config["Momo:QueryEndpoint"] ?? "https://test-payment.momo.vn/v2/gateway/api/query";
                var queryRequestId = Guid.NewGuid().ToString();

                // Build raw signature for query
                var rawSignature = $"accessKey={_accessKey}&orderId={orderId}&partnerCode={_partnerCode}&requestId={queryRequestId}";
                var signature = Security.ComputeHmacSha256(rawSignature, _secretKey);

                var payload = new
                {
                    partnerCode = _partnerCode,
                    accessKey = _accessKey,
                    requestId = queryRequestId,
                    orderId = orderId,
                    signature = signature,
                    lang = "vi"
                };

                var json = JsonSerializer.Serialize(payload);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await _http.PostAsync(queryEndpoint, httpContent);
                var respBody = await resp.Content.ReadAsStringAsync();

                _logger.LogInformation("MoMo Query Response: {Response}", respBody);

                return respBody;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying MoMo transaction status");
                throw;
            }
        }
    }
}