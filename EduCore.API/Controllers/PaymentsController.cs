using EduCore.Application.Features.Payments.DTOs;
using EduCore.Application.Features.Payments.Interfaces;
using EduCore.Application.Common.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduCore.Shared.Constants;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(
        IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<PaymentResponse>> Create(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _paymentService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Student}")]
    public async Task<ActionResult<PagedResult<PaymentResponse>>> GetAll(
        [FromQuery] PaginationParameters parameters,
    CancellationToken cancellationToken)
    {
        var payments = await _paymentService.GetAllAsync(parameters, cancellationToken);

        return Ok(payments);
    }


    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Student}")]
    public async Task<ActionResult<PaymentResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetByIdAsync(
            id,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("fee/{studentFeeId:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IEnumerable<PaymentResponse>>> GetByStudentFee(
        int studentFeeId,
        CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetByStudentFeeAsync(
            studentFeeId,
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<PaymentResponse>> Update(
    int id,
    [FromBody] UpdatePaymentRequest request,
    CancellationToken cancellationToken)
    {
        var result = await _paymentService.UpdateAsync(
            id,
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Cancel(
        int id,
        CancellationToken cancellationToken)
    {
        await _paymentService.CancelAsync(
            id,
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:int}/soft-delete")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> SoftDelete(
        int id,
        CancellationToken cancellationToken)
    {
        await _paymentService.SoftDeleteAsync(id, cancellationToken);
        return NoContent();
    }

}