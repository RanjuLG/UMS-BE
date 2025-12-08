using System.Security.Claims;
using UMS_BE.Models;

namespace UMS_BE.Services.Interfaces;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(User user, string? clientId = null);
    Task<string> GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
