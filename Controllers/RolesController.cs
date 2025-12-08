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
public class RolesController : ControllerBase
{
    private readonly IRoleRepository _roleRepository;

    public RolesController(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll()
    {
        var roles = await _roleRepository.GetAllAsync();
        var roleDtos = roles.Select(r => new RoleDto
        {
            RoleId = r.RoleId,
            Name = r.Name,
            Description = r.Description,
            IsActive = r.IsActive,
            PlatformId = r.PlatformId,
            PlatformName = r.Platform.Name
        });

        return Ok(roleDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetById(int id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            return NotFound();

        return Ok(new RoleDto
        {
            RoleId = role.RoleId,
            Name = role.Name,
            Description = role.Description,
            IsActive = role.IsActive,
            PlatformId = role.PlatformId,
            PlatformName = role.Platform.Name
        });
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleRequest request)
    {
        var currentUserId = User.GetCurrentUserId();
        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
            PlatformId = request.PlatformId,
            IsActive = true
        };

        var created = await _roleRepository.CreateAsync(role, currentUserId);
        return CreatedAtAction(nameof(GetById), new { id = created.RoleId }, new RoleDto
        {
            RoleId = created.RoleId,
            Name = created.Name,
            Description = created.Description,
            IsActive = created.IsActive,
            PlatformId = created.PlatformId,
            PlatformName = created.Platform.Name
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RoleDto>> Update(int id, [FromBody] UpdateRoleRequest request)
    {
        var currentUserId = User.GetCurrentUserId();
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            return NotFound();

        if (!string.IsNullOrEmpty(request.Name))
            role.Name = request.Name;
        if (!string.IsNullOrEmpty(request.Description))
            role.Description = request.Description;
        if (request.IsActive.HasValue)
            role.IsActive = request.IsActive.Value;

        var updated = await _roleRepository.UpdateAsync(role, currentUserId);
        return Ok(new RoleDto
        {
            RoleId = updated.RoleId,
            Name = updated.Name,
            Description = updated.Description,
            IsActive = updated.IsActive,
            PlatformId = updated.PlatformId,
            PlatformName = updated.Platform.Name
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = User.GetCurrentUserId();
        var result = await _roleRepository.DeleteAsync(id, currentUserId);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleRequest request)
    {
        var result = await _roleRepository.AssignRoleToUserAsync(request.UserId, request.RoleId);
        if (!result)
            return BadRequest(new { message = "Role already assigned or invalid user/role" });

        return Ok(new { message = "Role assigned successfully" });
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemoveRoleFromUser([FromBody] AssignRoleRequest request)
    {
        var result = await _roleRepository.RemoveRoleFromUserAsync(request.UserId, request.RoleId);
        if (!result)
            return BadRequest(new { message = "Role not found for user" });

        return Ok(new { message = "Role removed successfully" });
    }

    [HttpGet("{id}/permissions")]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetRolePermissions(int id)
    {
        var permissions = await _roleRepository.GetRolePermissionsAsync(id);
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
}
