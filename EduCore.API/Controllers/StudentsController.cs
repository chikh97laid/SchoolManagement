using EduCore.Application.Features.StudentManagement.DTOs;
using EduCore.Application.Features.StudentManagement.DTOs.Student;
using EduCore.Application.Features.StudentManagement.Interfaces;
using EduCore.Application.Common.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduCore.Shared.Constants;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(
        IStudentService studentService)
    {
        _studentService = studentService;        
    }

    // POST: api/StudentManagement
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<ActionResult<CreateStudentResponse>> CreateStudent(
        [FromBody] CreateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _studentService.CreateStudentAsync(
                request,
                cancellationToken);

        return CreatedAtAction(
                nameof(GetStudentById),
                new { id = result.StudentId },
                result);
    }

    // GET: api/StudentManagement
    [Authorize(Roles = $"{Roles.Admin},{Roles.Teacher}")]
    [HttpGet]
    public async Task<ActionResult<PagedResult<StudentResponse>>> GetStudents(
        [FromQuery] PaginationParameters parameters,
        CancellationToken cancellationToken)
    {
        var result =
            await _studentService.GetStudentsAsync(
                parameters,
                cancellationToken);

        return Ok(result);
    }

    // GET: api/StudentManagement/5
    [Authorize(Roles = $"{Roles.Admin},{Roles.Teacher},{Roles.Student}")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudentResponse>> GetStudentById(
        int id,
        CancellationToken cancellationToken)
    {
        var result =
            await _studentService.GetStudentByIdAsync(
                id,
                cancellationToken);

        return Ok(result);
    }

    // PUT: api/StudentManagement/5
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<StudentResponse>> UpdateStudent(
        int id,
        [FromBody] UpdateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _studentService.UpdateStudentAsync(
                id,
                request,
                cancellationToken);

        return Ok(result);
    }

    // PATCH: api/StudentManagement/5/deactivate
    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateStudentAccount(
        int id,
        CancellationToken cancellationToken)
    {
        await _studentService
            .DeactivateStudentAccountAsync(
                id,
                cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("{id:int}/soft-delete")]
    public async Task<IActionResult> SoftDeleteStudent(
        int id,
        CancellationToken cancellationToken)
    {
        await _studentService.SoftDeleteStudentAsync(id, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(StudentImportSettings.MaxRequestSizeBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = StudentImportSettings.MaxRequestSizeBytes)]
    public async Task<ActionResult<ImportStudentsResponse>> ImportStudents(
    IFormFile file,
    CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Excel file is required.");

        if (file.Length > StudentImportSettings.MaxFileSizeBytes)
            return StatusCode(StatusCodes.Status413PayloadTooLarge);

        if (!Path.GetExtension(file.FileName)
            .Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(
                "Only .xlsx Excel files are supported.");
        }

        await using var stream =
            file.OpenReadStream();

        var result =
            await _studentService
                .ImportStudentsFromExcelAsync(
                    stream,
                    cancellationToken);

        return Ok(result);
    }

}
