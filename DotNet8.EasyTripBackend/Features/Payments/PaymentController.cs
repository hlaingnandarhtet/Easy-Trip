using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.Payments
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        [HttpGet("methods")]
        public async Task<ActionResult<List<PaymentMethodResponseModel>>> GetPaymentMethods()
        {
            var result = await _paymentService.GetPaymentMethodsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentMethod(long id)
        {
            var result = await _paymentService.GetPaymentMethodByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePaymentMethod([FromBody] PaymentMethodRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(request.PaymentType)) return BadRequest("PaymentType is required.");
            if (string.IsNullOrWhiteSpace(request.AccountName)) return BadRequest("AccountName is required.");
            if (string.IsNullOrWhiteSpace(request.AccountNumber)) return BadRequest("AccountNumber is required.");

            var created = await _paymentService.CreatePaymentMethodAsync(request);
            return CreatedAtAction(nameof(GetPaymentMethod), new { id = created.Id }, created);
        }

        [HttpPut("{id}/updates")]
        public async Task<IActionResult> UpdatePaymentMethod(long id, [FromBody] PaymentMethodRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(request.PaymentType)) return BadRequest("PaymentType is required.");
            if (string.IsNullOrWhiteSpace(request.AccountName)) return BadRequest("AccountName is required.");
            if (string.IsNullOrWhiteSpace(request.AccountNumber)) return BadRequest("AccountNumber is required.");

            var updated = await _paymentService.UpdatePaymentMethodAsync(id, request);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id}/delete")]
        public async Task<IActionResult> DeletePaymentMethod(long id)
        {
            var success = await _paymentService.DeletePaymentMethodAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }
    }
}
