using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Model.Entities;
using WebApp.Model.Request;
using WebApp.Service.Interfaces;

namespace WebApp.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            // get userId from JWT typically
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
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();

                // signature header commonly "signature" (check MoMo doc)
                var signature = Request.Headers["signature"].FirstOrDefault() ?? Request.Headers["Signature"].FirstOrDefault() ?? string.Empty;

                await _service.HandleMomoNotifyAsync(body, signature);

                return Ok(new
                {
                    partnerCode = "MOMO",
                    requestId = "...",
                    orderId = "...",
                    resultCode = 0,
                    message = "Success"
                });
            } catch (Exception ex)
            {
                _logger.LogError($"Momo notify error: {ex.Message}");
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
            // redirect client to frontend page and show success/fail
            var frontendUrl = _config["Frontend:Url"];
            var redirect = $"{frontendUrl}/payment-result?orderId={orderId}&requestId={requestId}&resultCode={resultCode}";
            return Redirect(redirect);
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
            var userIdClaim = User.FindFirst("id");
            if(userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);
            var result = await _service.RetryPaymentAsync(paymentId, userId);
            return Ok(result);
        }
    }
}
