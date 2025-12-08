using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UMS_BE.Extensions;
using UMS_BE.Models;
using UMS_BE.Models.DTOs;
using UMS_BE.Repositories.Interfaces;

namespace UMS_BE.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminPolicy")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionRepository _permissionRepository;

    public PermissionsController(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetAll()
    {
        var permissions = await _permissionRepository.GetAllAsync();
        var permissionDtos = permissions.Select(p => new PermissionDto
        {
            PermissionId = p.PermissionId,
            Name = p.Name,
            Description = p.Description,
            Resource = p.Resource,
            Action = p.Action,
            PlatformId = p.PlatformId,
            PlatformName = p.Platform.Name,
            IsActive = p.IsActive
        });

        return Ok(permissionDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PermissionDto>> GetById(int id)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        if (permission == null)
            return NotFound();

        return Ok(new PermissionDto
        {
            PermissionId = permission.PermissionId,
            Name = permission.Name,
            Description = permission.Description,
            Resource = permission.Resource,
            Action = permission.Action,
            PlatformId = permission.PlatformId,
            PlatformName = permission.Platform.Name,
            IsActive = permission.IsActive
        });
    }

    [HttpGet("platform/{platformId}")]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetByPlatform(int platformId)
    {
        var permissions = await _permissionRepository.GetByPlatformIdAsync(platformId);
        var permissionDtos = permissions.Select(p => new PermissionDto
        {
            PermissionId = p.PermissionId,
            Name = p.Name,
            Description = p.Description,
            Resource = p.Resource,
            Action = p.Action,
            PlatformId = p.PlatformId,
            PlatformName = p.Platform.Name,
            IsActive = p.IsActive
        });

        return Ok(permissionDtos);
    }

    [HttpPost]
    public async Task<ActionResult<PermissionDto>> Create([FromBody] CreatePermissionRequest request)
    {
        var currentUserId = User.GetCurrentUserId();
        var permission = new Permission
        {
            Name = request.Name,
            Description = request.Description,
            Resource = request.Resource,
            Action = request.Action,
            PlatformId = request.PlatformId,
            IsActive = true
        };

        var created = await _permissionRepository.CreateAsync(permission, currentUserId);
        return CreatedAtAction(nameof(GetById), new { id = created.PermissionId }, new PermissionDto
        {
            PermissionId = created.PermissionId,
            Name = created.Name,
            Description = created.Description,
            Resource = created.Resource,
            Action = created.Action,
            PlatformId = created.PlatformId,
            PlatformName = created.Platform.Name,
            IsActive = created.IsActive
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PermissionDto>> Update(int id, [FromBody] UpdatePermissionRequest request)
    {
        var currentUserId = User.GetCurrentUserId();
        var permission = await _permissionRepository.GetByIdAsync(id);
        if (permission == null)
            return NotFound();

        if (!string.IsNullOrEmpty(request.Name))
            permission.Name = request.Name;
        if (!string.IsNullOrEmpty(request.Description))
            permission.Description = request.Description;
        if (!string.IsNullOrEmpty(request.Resource))
            permission.Resource = request.Resource;
        if (!string.IsNullOrEmpty(request.Action))
            permission.Action = request.Action;
        if (request.IsActive.HasValue)
            permission.IsActive = request.IsActive.Value;

        var updated = await _permissionRepository.UpdateAsync(permission, currentUserId);
        return Ok(new PermissionDto
        {
            PermissionId = updated.PermissionId,
            Name = updated.Name,
            Description = updated.Description,
            Resource = updated.Resource,
            Action = updated.Action,
            PlatformId = updated.PlatformId,
            PlatformName = updated.Platform.Name,
            IsActive = updated.IsActive
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = User.GetCurrentUserId();
        var result = await _permissionRepository.DeleteAsync(id, currentUserId);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignPermissionToRole([FromBody] AssignPermissionRequest request)
    {
        var result = await _permissionRepository.AssignPermissionToRoleAsync(request.RoleId, request.PermissionId);
        if (!result)
            return BadRequest(new { message = "Permission already assigned or invalid role/permission" });

        return Ok(new { message = "Permission assigned successfully" });
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemovePermissionFromRole([FromBody] AssignPermissionRequest request)
    {
        var result = await _permissionRepository.RemovePermissionFromRoleAsync(request.RoleId, request.PermissionId);
        if (!result)
            return BadRequest(new { message = "Permission not found for role" });

        return Ok(new { message = "Permission removed successfully" });
    }
}
