using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHealthInsurance.Api.DTOs;
using SmartHealthInsurance.Api.Services;
using System.Security.Claims;

namespace SmartHealthInsurance.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("premium")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> PayPremium(PremiumPaymentDto dto)
        {
            try
            {
                var result = await _paymentService.PayPremiumAsync(dto);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("my-payments")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyPayments()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var payments = await _paymentService.GetCustomerPaymentsAsync(userId);
            return Ok(payments);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,ClaimsOfficer")]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments);
        }
    }
}
