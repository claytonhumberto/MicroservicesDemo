using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.DTOs;
using Payments.Application.Services;

namespace Payments.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Produces("application/json")]
public class PaymentsController(PaymentService paymentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await paymentService.GetPagedAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await paymentService.GetByIdAsync(id, cancellationToken);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken cancellationToken = default)
    {
        var payments = await paymentService.GetByOrderIdAsync(orderId, cancellationToken);
        return Ok(payments);
    }

    /// <summary>
    /// Process a payment. Use X-Provider-Behavior header to demo resilience:
    /// success (default) | fail | timeout | slow
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Process(
        [FromBody] ProcessPaymentRequest request,
        [FromHeader(Name = "X-Provider-Behavior")] string? providerBehavior = null,
        CancellationToken cancellationToken = default)
    {
        var requestWithBehavior = request with { ProviderBehavior = providerBehavior };
        var payment = await paymentService.ProcessAsync(requestWithBehavior, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
    }
}
