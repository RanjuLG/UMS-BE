using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using UMS_BE.Repositories.Interfaces;

namespace UMS_BE.IdentityServer;

public class CustomProfileService : IProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly IPlatformRepository _platformRepository;

    public CustomProfileService(IUserRepository userRepository, IPlatformRepository platformRepository)
    {
        _userRepository = userRepository;
        _platformRepository = platformRepository;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var subjectId = context.Subject.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(subjectId) || !int.TryParse(subjectId, out var userId))
            return;

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.IsActive)
            return;

        var claims = new List<Claim>
        {
            new Claim("sub", user.UserId.ToString()),
            new Claim("email", user.Email),
            new Claim("name", $"{user.FirstName} {user.LastName}"),
            new Claim("given_name", user.FirstName),
            new Claim("family_name", user.LastName),
            new Claim("username", user.UserName)
        };

        // Get roles
        var roles = await _userRepository.GetUserRolesAsync(userId);
        foreach (var role in roles)
        {
            claims.Add(new Claim("role", role.Name));
        }

        // Get client ID to determine platform
        var clientId = context.Client?.ClientId;
        if (!string.IsNullOrEmpty(clientId))
        {
            var platform = await _platformRepository.GetByClientIdAsync(clientId);
            if (platform != null)
            {
                claims.Add(new Claim("platform_id", platform.PlatformId.ToString()));
                claims.Add(new Claim("platform_name", platform.Name));

                // Get permissions for this platform
                var permissions = await _userRepository.GetUserPermissionsForPlatformAsync(userId, platform.PlatformId);
                foreach (var permission in permissions)
                {
                    claims.Add(new Claim("permission", $"{permission.Resource}:{permission.Action}"));
                }
            }
        }

        context.IssuedClaims.AddRange(claims);
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var subjectId = context.Subject.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(subjectId) || !int.TryParse(subjectId, out var userId))
        {
            context.IsActive = false;
            return;
        }

        var user = await _userRepository.GetByIdAsync(userId);
        context.IsActive = user != null && user.IsActive;
    }
}
