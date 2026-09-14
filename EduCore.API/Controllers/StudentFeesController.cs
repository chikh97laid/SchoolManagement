using EduCore.Application.Features.StudentFees.DTOs;
using EduCore.Application.Features.StudentFees.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduCore.Shared.Constants;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentFeesController : ControllerBase
{
    private readonly IStudentFeeService _studentFeeService;

    public StudentFeesController(
        IStudentFeeService studentFeeService)
    {
        _studentFeeService = studentFeeService;
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<StudentFeeResponse>> Create(
        [FromBody] CreateStudentFeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _studentFeeService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Teacher}")]
    public async Task<ActionResult<StudentFeeResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _studentFeeService.GetByIdAsync(
            id,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IEnumerable<StudentFeeResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _studentFeeService.GetAllAsync(
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("student/{studentId:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IEnumerable<StudentFeeResponse>>> GetByStudent(
        int studentId,
        CancellationToken cancellationToken)
    {
        var result = await _studentFeeService.GetByStudentAsync(
            studentId,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("enrollment/{enrollmentId:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IEnumerable<StudentFeeResponse>>> GetByEnrollment(
        int enrollmentId,
        CancellationToken cancellationToken)
    {
        var result = await _studentFeeService.GetByEnrollmentAsync(
            enrollmentId,
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<StudentFeeResponse>> Update(
        int id,
        [FromBody] UpdateStudentFeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _studentFeeService.UpdateAsync(
            id,
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id:int}/soft-delete")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> SoftDelete(
        int id,
        CancellationToken cancellationToken)
    {
        await _studentFeeService.SoftDeleteAsync(
            id,
            cancellationToken);

        return NoContent();
    }
}