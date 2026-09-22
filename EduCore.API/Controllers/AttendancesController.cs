using EduCore.Application.Features.Attendances.DTOs;
using EduCore.Application.Features.Attendances.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduCore.Shared.Constants;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendancesController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendancesController(
        IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<AttendanceResponse>> Create(
        [FromBody] CreateAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Teacher}")]
    public async Task<ActionResult<IEnumerable<AttendanceResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetAllAsync(
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Teacher},{Roles.Student}")]
    public async Task<ActionResult<AttendanceResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetByIdAsync(
            id,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("enrollment/{enrollmentId:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IEnumerable<AttendanceResponse>>> GetByEnrollment(
        int enrollmentId,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetByEnrollmentAsync(
            enrollmentId,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("student/{studentId:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IEnumerable<AttendanceResponse>>> GetByStudent(
        int studentId,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetByStudentAsync(
            studentId,
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<AttendanceResponse>> Update(
        int id,
        [FromBody] UpdateAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.UpdateAsync(
            id,
            request,
            cancellationToken);

        return Ok(result);
    }
}