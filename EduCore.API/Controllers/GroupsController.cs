using EduCore.Application.Features.Groups.DTOs;
using EduCore.Application.Features.Groups.Interfaces;
using EduCore.Application.Common.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduCore.Shared.Constants;

namespace EduCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GroupsController : ControllerBase
    {

        private readonly IGroupService _groupService;

        public GroupsController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<GroupResponse>> Create(
            [FromBody] CreateGroupRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _groupService.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Teacher}")]
        public async Task<ActionResult<GroupResponse>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _groupService.GetByIdAsync(
                id,
                cancellationToken);

            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Teacher}")]
        public async Task<ActionResult<PagedResult<GroupResponse>>> GetAll(
            [FromQuery] PaginationParameters parameters,
            CancellationToken cancellationToken)
        {
            var result = await _groupService.GetAllAsync(
                parameters,
                cancellationToken);

            return Ok(result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<GroupResponse>> Update(
            int id,
            [FromBody] UpdateGroupRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _groupService.UpdateAsync(
                id,
                request,
                cancellationToken);

            return Ok(result);
        }

        [HttpPatch("{id:int}/deactivate")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Deactivate(
            int id,
            CancellationToken cancellationToken)
        {
            await _groupService.DeactivateAsync(
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
            await _groupService.ActivateAsync(
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
            await _groupService.SoftDeleteAsync(id, cancellationToken);
            return NoContent();
        }

    }
}
