using EduCore.Application.Features.Enrollments.DTOs;
using EduCore.Application.Features.Enrollments.Interfaces;
using EduCore.Application.Common.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduCore.Shared.Constants;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(
        IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<EnrollmentResponse>> Create(
        [FromBody] CreateEnrollmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Teacher},{Roles.Student}")]
    public async Task<ActionResult<EnrollmentResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.GetByIdAsync(
            id,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin}")]
    public async Task<ActionResult<PagedResult<EnrollmentResponse>>> GetAll(
        [FromQuery] PaginationParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.GetAllAsync(
            parameters,
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Deactivate(
        int id,
        CancellationToken cancellationToken)
    {
        await _enrollmentService.DeactivateAsync(
            id,
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:int}/activate")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Activate(
        int id,
        CancellationToken cancellationToken)
    {
        await _enrollmentService.ActivateAsync(
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
        await _enrollmentService.SoftDeleteAsync(id, cancellationToken);
        return NoContent();
    }
}