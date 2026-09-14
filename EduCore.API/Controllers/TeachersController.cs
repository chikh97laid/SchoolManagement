using EduCore.Application.Features.Teachers.DTOs;
using EduCore.Application.Features.Teachers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TeachersController : ControllerBase
{
    private readonly ITeacherService _teacherService;

    public TeachersController(ITeacherService teacherService)
    {
        _teacherService = teacherService;
    }

    // GET: api/teachers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeacherResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var teachers = await _teacherService.GetAllAsync(
            cancellationToken);

        return Ok(teachers);
    }

    // GET: api/teachers/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TeacherResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var teacher = await _teacherService.GetByIdAsync(
            id,
            cancellationToken);

        return Ok(teacher);
    }

    // POST: api/teachers
    [HttpPost]
    public async Task<ActionResult<TeacherResponse>> Create(
        [FromBody] CreateTeacherRequest request,
        CancellationToken cancellationToken)
    {
        var teacher = await _teacherService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = teacher.TeacherId },
            teacher);
    }

    // PUT: api/teachers/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TeacherResponse>> Update(
        int id,
        [FromBody] UpdateTeacherRequest request,
        CancellationToken cancellationToken)
    {
        var teacher = await _teacherService.UpdateAsync(
            id,
            request,
            cancellationToken);

        return Ok(teacher);
    }

    // PATCH: api/teachers/5/deactivate
    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(
        int id,
        CancellationToken cancellationToken)
    {
        await _teacherService.DeactivateAsync(
            id,
            cancellationToken);

        return NoContent();
    }

    // PATCH: api/teachers/5/activate
    [HttpPatch("{id:int}/activate")]
    public async Task<IActionResult> Activate(
        int id,
        CancellationToken cancellationToken)
    {
        await _teacherService.ActivateAsync(
            id,
            cancellationToken);

        return NoContent();
    }
}