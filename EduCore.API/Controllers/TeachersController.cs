using EduCore.Application.Common.Pagination;
using EduCore.Application.Features.Teachers.DTOs;
using EduCore.Application.Features.Teachers.Interfaces;
using EduCore.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeachersController : ControllerBase
{
    private readonly ITeacherService _teacherService;

    public TeachersController(ITeacherService teacherService)
    {
        _teacherService = teacherService;
    }

    // GET: api/teachers
    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<ActionResult<PagedResult<TeacherResponse>>> GetAll(
    [FromQuery] PaginationParameters parameters,
    CancellationToken cancellationToken)
    {
        var result = await _teacherService.GetAllAsync(
            parameters,
            cancellationToken);

        return Ok(result);
    }


    // GET: api/teachers/5
    [Authorize(Roles = $"{Roles.Admin},{Roles.Teacher}, {Roles.Student}")]
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
    [Authorize(Roles = Roles.Admin)]
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
    [Authorize(Roles = Roles.Admin)]
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
    [Authorize(Roles = Roles.Admin)]
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
    [Authorize(Roles = Roles.Admin)]
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

    [HttpPatch("{id:int}/soft-delete")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> SoftDelete(
        int id,
        CancellationToken cancellationToken)
    {
        await _teacherService.SoftDeleteAsync(id, cancellationToken);
        return NoContent();
    }
}