using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Model.Entities;
using WebApp.Model.Request;
using WebApp.Service.Interfaces;

namespace WebApp.Controller.Controllers
{
    [Route("api/payment")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _service;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService service, IConfiguration config, ILogger<PaymentController> logger)
        {
            _logger = logger;
            _service = service;
            _config = config;
        }

        [HttpPost("momo/create")]
        public async Task<IActionResult> CreateMomo([FromBody] MomoPaymentRequest request)
        {
            var userIdClaim = User.FindFirst("id");
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);

            var momoResp = await _service.CreateMomoPaymentAsync(request, userId);
            return Ok(momoResp);
        }

        // notify url (MoMo server -> call this)
        [HttpPost("momo/notify")]
        [AllowAnonymous]
        public async Task<IActionResult> MomoNotify()
        {
            try
            {
                _logger.LogInformation("=== MoMo IPN Received ===");
                _logger.LogInformation("Headers: {Headers}", string.Join(", ", Request.Headers.Select(h => $"{h.Key}={h.Value}")));

                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();

                _logger.LogInformation("MoMo IPN Body: {Body}", body);

                var signature = Request.Headers["signature"].FirstOrDefault()
                    ?? Request.Headers["Signature"].FirstOrDefault()
                    ?? string.Empty;

                _logger.LogInformation("MoMo IPN Signature: {Signature}", signature);

                await _service.HandleMomoNotifyAsync(body, signature);

                _logger.LogInformation("MoMo IPN processed successfully");

                return Ok(new
                {
                    partnerCode = "MOMO",
                    requestId = "...",
                    orderId = "...",
                    resultCode = 0,
                    message = "Success"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Momo notify error: {Message}", ex.Message);
                return Ok(new
                {
                    resultCode = 1,
                    message = ex.Message
                });
            }
        }

        // return url (user redirected back) — optional for client UX
        [HttpGet("momo/return")]
        public IActionResult MomoReturn([FromQuery] string orderId, [FromQuery] string requestId, [FromQuery] int resultCode)
        {
            try
            {
                var actualOrderId = orderId?.Split('_')[0] ?? orderId;

                _logger.LogInformation("MoMo Return: orderId={OrderId}, actualOrderId={ActualOrderId}, resultCode={ResultCode}",
                    orderId, actualOrderId, resultCode);

                var frontendUrl = _config["Frontend:Url"] ?? "http://localhost:5173";
                var redirect = $"{frontendUrl}/payment-result?orderId={actualOrderId}&requestId={requestId}&resultCode={resultCode}";

                return Redirect(redirect);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MomoReturn");
                var frontendUrl = _config["Frontend:Url"] ?? "http://localhost:5173";
                return Redirect($"{frontendUrl}/payment-result?resultCode=1&error=parse_error");
            }
        }

        [HttpGet("orderId/{orderId}")]
        public async Task<IActionResult> GetByOrder(int orderId)
        {
            var result = await _service.GetPaymentsByOrderIdAsync(orderId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _service.GetPaymentByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("{paymentId}/retry")]
        public async Task<IActionResult> Retry(int paymentId)
        {
            try
            {
                var userIdClaim = User.FindFirst("id");
                if (userIdClaim == null) return Unauthorized();
                var userId = int.Parse(userIdClaim.Value);

                _logger.LogInformation("Retry payment {PaymentId} for user {UserId}", paymentId, userId);

                var result = await _service.RetryPaymentAsync(paymentId, userId);

                _logger.LogInformation("Retry result: PayUrl={PayUrl}, RequestId={RequestId}", result.PayUrl, result.RequestId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying payment {PaymentId}", paymentId);
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            var userIdClaim = User.FindFirst("id");
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);

            var success = await _service.ConfirmPaymentAsync(request.OrderId, request.ResultCode, userId);
            return Ok(new { success, message = success ? "Payment confirmed" : "Payment failed or already processed" });
        }
        public class ConfirmPaymentRequest
        {
            public int OrderId { get; set; }
            public int ResultCode { get; set; }
        }


    }
}
