using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using UMS_BE.Extensions;
using UMS_BE.Models;
using UMS_BE.Models.DTOs;
using UMS_BE.Repositories.Interfaces;

namespace UMS_BE.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminPolicy")]
public class PlatformsController : ControllerBase
{
    private readonly IPlatformRepository _platformRepository;

    public PlatformsController(IPlatformRepository platformRepository)
    {
        _platformRepository = platformRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlatformDto>>> GetAll()
    {
        var platforms = await _platformRepository.GetAllAsync();
        var platformDtos = platforms.Select(p => new PlatformDto
        {
            PlatformId = p.PlatformId,
            Name = p.Name,
            ClientId = p.ClientId,
            Description = p.Description,
            RedirectUris = string.IsNullOrEmpty(p.RedirectUris) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(p.RedirectUris) ?? new List<string>(),
            PostLogoutRedirectUris = string.IsNullOrEmpty(p.PostLogoutRedirectUris) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(p.PostLogoutRedirectUris) ?? new List<string>(),
            AllowedScopes = string.IsNullOrEmpty(p.AllowedScopes) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(p.AllowedScopes) ?? new List<string>(),
            IsActive = p.IsActive
        });

        return Ok(platformDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlatformDto>> GetById(int id)
    {
        var platform = await _platformRepository.GetByIdAsync(id);
        if (platform == null)
            return NotFound();

        return Ok(new PlatformDto
        {
            PlatformId = platform.PlatformId,
            Name = platform.Name,
            ClientId = platform.ClientId,
            Description = platform.Description,
            RedirectUris = string.IsNullOrEmpty(platform.RedirectUris) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(platform.RedirectUris) ?? new List<string>(),
            PostLogoutRedirectUris = string.IsNullOrEmpty(platform.PostLogoutRedirectUris) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(platform.PostLogoutRedirectUris) ?? new List<string>(),
            AllowedScopes = string.IsNullOrEmpty(platform.AllowedScopes) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(platform.AllowedScopes) ?? new List<string>(),
            IsActive = platform.IsActive
        });
    }

    [HttpPost]
    public async Task<ActionResult<PlatformDto>> Create([FromBody] CreatePlatformRequest request)
    {
        var currentUserId = User.GetCurrentUserId();
        var clientSecret = Guid.NewGuid().ToString("N");

        var platform = new Platform
        {
            Name = request.Name,
            ClientId = $"platform_{Guid.NewGuid():N}",
            ClientSecret = clientSecret,
            Description = request.Description,
            RedirectUris = JsonSerializer.Serialize(request.RedirectUris),
            PostLogoutRedirectUris = JsonSerializer.Serialize(request.PostLogoutRedirectUris),
            AllowedScopes = JsonSerializer.Serialize(request.AllowedScopes),
            IsActive = true
        };

        var created = await _platformRepository.CreateAsync(platform, currentUserId);
        return CreatedAtAction(nameof(GetById), new { id = created.PlatformId }, new PlatformDto
        {
            PlatformId = created.PlatformId,
            Name = created.Name,
            ClientId = created.ClientId,
            Description = created.Description,
            RedirectUris = request.RedirectUris,
            PostLogoutRedirectUris = request.PostLogoutRedirectUris,
            AllowedScopes = request.AllowedScopes,
            IsActive = created.IsActive
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PlatformDto>> Update(int id, [FromBody] UpdatePlatformRequest request)
    {
        var currentUserId = User.GetCurrentUserId();
        var platform = await _platformRepository.GetByIdAsync(id);
        if (platform == null)
            return NotFound();

        if (!string.IsNullOrEmpty(request.Name))
            platform.Name = request.Name;
        if (!string.IsNullOrEmpty(request.Description))
            platform.Description = request.Description;
        if (request.RedirectUris != null)
            platform.RedirectUris = JsonSerializer.Serialize(request.RedirectUris);
        if (request.PostLogoutRedirectUris != null)
            platform.PostLogoutRedirectUris = JsonSerializer.Serialize(request.PostLogoutRedirectUris);
        if (request.AllowedScopes != null)
            platform.AllowedScopes = JsonSerializer.Serialize(request.AllowedScopes);
        if (request.IsActive.HasValue)
            platform.IsActive = request.IsActive.Value;

        var updated = await _platformRepository.UpdateAsync(platform, currentUserId);
        return Ok(new PlatformDto
        {
            PlatformId = updated.PlatformId,
            Name = updated.Name,
            ClientId = updated.ClientId,
            Description = updated.Description,
            RedirectUris = string.IsNullOrEmpty(updated.RedirectUris) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(updated.RedirectUris) ?? new List<string>(),
            PostLogoutRedirectUris = string.IsNullOrEmpty(updated.PostLogoutRedirectUris) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(updated.PostLogoutRedirectUris) ?? new List<string>(),
            AllowedScopes = string.IsNullOrEmpty(updated.AllowedScopes) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(updated.AllowedScopes) ?? new List<string>(),
            IsActive = updated.IsActive
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = User.GetCurrentUserId();
        var result = await _platformRepository.DeleteAsync(id, currentUserId);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
