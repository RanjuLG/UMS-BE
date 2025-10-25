using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UMS_BE.Extensions;
using UMS_BE.Models.DTOs;
using UMS_BE.Repositories.Interfaces;
using UMS_BE.Services.Interfaces;

namespace UMS_BE.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminPolicy")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;

    public UsersController(IUserService userService, IUserRepository userRepository)
    {
        _userService = userService;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        var userDtos = users.Select(u => new UserDto
        {
            UserId = u.UserId,
            UserName = u.UserName,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            IsActive = u.IsActive,
            Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
        });

        return Ok(userDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(new UserDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsActive = user.IsActive,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateAsync(
                request.UserName,
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password);

            return CreatedAtAction(nameof(GetById), new { id = user.UserId }, new UserDto
            {
                UserId = user.UserId,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsActive = user.IsActive
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.UpdateAsync(
                id,
                request.UserName,
                request.FirstName,
                request.LastName,
                request.Email,
                request.IsActive);

            return Ok(new UserDto
            {
                UserId = user.UserId,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsActive = user.IsActive,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = User.GetCurrentUserId();
        var result = await _userRepository.DeleteAsync(id, currentUserId);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPut("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        var result = await _userService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);
        if (!result)
            return BadRequest(new { message = "Invalid current password" });

        return Ok(new { message = "Password changed successfully" });
    }

    [HttpGet("{id}/roles")]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetUserRoles(int id)
    {
        var roles = await _userRepository.GetUserRolesAsync(id);
        var roleDtos = roles.Select(r => new RoleDto
        {
            RoleId = r.RoleId,
            Name = r.Name,
            Description = r.Description,
            IsActive = r.IsActive
        });

        return Ok(roleDtos);
    }
}
