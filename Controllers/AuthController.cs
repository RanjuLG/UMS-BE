using Duende.IdentityServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UMS_BE.Models.DTOs;
using UMS_BE.Services.Interfaces;

namespace UMS_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.ValidateUserAsync(request.Email, request.Password);
        if (user == null)
        {
            return Ok(new LoginResponse
            {
                Success = false,
                Message = "Invalid email or password"
            });
        }

        // Create claims for the cookie
        var claims = new List<Claim>
        {
            new Claim("sub", user.UserId.ToString()),
            new Claim("email", user.Email),
            new Claim("name", $"{user.FirstName} {user.LastName}")
        };

        var identity = new ClaimsIdentity(claims, IdentityServerConstants.DefaultCookieAuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            IdentityServerConstants.DefaultCookieAuthenticationScheme,
            principal);

        return Ok(new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            User = new UserDto
            {
                UserId = user.UserId,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsActive = user.IsActive,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            }
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);
        return Ok(new { message = "Logout successful" });
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var existingUser = await _userService.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email already exists" });
            }

            var user = await _userService.CreateAsync(
                request.UserName,
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password);

            return CreatedAtAction(nameof(Register), new UserDto
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
}
