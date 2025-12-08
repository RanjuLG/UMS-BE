using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using UMS_BE.Models;
using UMS_BE.Repositories.Interfaces;
using UMS_BE.Services.Interfaces;

namespace UMS_BE.Services.Implementations;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly IPlatformRepository _platformRepository;

    public TokenService(
        IConfiguration configuration,
        IUserRepository userRepository,
        IPlatformRepository platformRepository)
    {
        _configuration = configuration;
        _userRepository = userRepository;
        _platformRepository = platformRepository;
    }

    public async Task<string> GenerateAccessTokenAsync(User user, string? clientId = null)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim("username", user.UserName),
            new Claim("name", $"{user.FirstName} {user.LastName}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add user's assigned platforms with redirect URIs
        var userPlatforms = await _userRepository.GetUserPlatformsAsync(user.UserId);
        var platformsList = userPlatforms.ToList();
        
        if (platformsList.Any())
        {
            // Add platforms as a JSON array
            var platformsData = platformsList.Select(p => new
            {
                platformId = p.PlatformId,
                name = p.Name,
                clientId = p.ClientId,
                redirectUris = string.IsNullOrEmpty(p.RedirectUris) ? new string[0] : JsonSerializer.Deserialize<string[]>(p.RedirectUris) ?? new string[0],
                postLogoutRedirectUris = string.IsNullOrEmpty(p.PostLogoutRedirectUris) ? new string[0] : JsonSerializer.Deserialize<string[]>(p.PostLogoutRedirectUris) ?? new string[0]
            }).ToList();
            
            claims.Add(new Claim("platforms", JsonSerializer.Serialize(platformsData)));
        }

        // Add roles
        var roles = await _userRepository.GetUserRolesAsync(user.UserId);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
            claims.Add(new Claim("role", role.Name));
        }

        // Add platform-specific permissions if clientId is provided
        if (!string.IsNullOrEmpty(clientId))
        {
            var platform = await _platformRepository.GetByClientIdAsync(clientId);
            if (platform != null)
            {
                claims.Add(new Claim("client_platform_id", platform.PlatformId.ToString()));
                claims.Add(new Claim("client_platform_name", platform.Name));

                // Get permissions for this platform
                var permissions = await _userRepository.GetUserPermissionsForPlatformAsync(user.UserId, platform.PlatformId);
                foreach (var permission in permissions)
                {
                    claims.Add(new Claim("permission", $"{permission.Resource}:{permission.Action}"));
                }
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Task<string> GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Task.FromResult(Convert.ToBase64String(randomNumber));
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
