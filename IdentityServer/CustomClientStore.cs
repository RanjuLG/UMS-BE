using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using UMS_BE.Repositories.Interfaces;
using System.Text.Json;

namespace UMS_BE.IdentityServer;

public class CustomClientStore : IClientStore
{
    private readonly IPlatformRepository _platformRepository;

    public CustomClientStore(IPlatformRepository platformRepository)
    {
        _platformRepository = platformRepository;
    }

    public async Task<Client?> FindClientByIdAsync(string clientId)
    {
        var platform = await _platformRepository.GetByClientIdAsync(clientId);
        if (platform == null || !platform.IsActive)
            return null;

        var redirectUris = string.IsNullOrEmpty(platform.RedirectUris) 
            ? new List<string>() 
            : JsonSerializer.Deserialize<List<string>>(platform.RedirectUris) ?? new List<string>();

        var postLogoutRedirectUris = string.IsNullOrEmpty(platform.PostLogoutRedirectUris)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(platform.PostLogoutRedirectUris) ?? new List<string>();

        var allowedScopes = string.IsNullOrEmpty(platform.AllowedScopes)
            ? new List<string> { "openid", "profile", "email" }
            : JsonSerializer.Deserialize<List<string>>(platform.AllowedScopes) ?? new List<string>();

        return new Client
        {
            ClientId = platform.ClientId,
            ClientName = platform.Name,
            ClientSecrets = { new Secret(platform.ClientSecret.Sha256()) },
            AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
            AllowedScopes = allowedScopes,
            RedirectUris = redirectUris,
            PostLogoutRedirectUris = postLogoutRedirectUris,
            AllowOfflineAccess = true,
            AccessTokenLifetime = 3600,
            RefreshTokenUsage = TokenUsage.ReUse,
            RefreshTokenExpiration = TokenExpiration.Sliding,
            SlidingRefreshTokenLifetime = 86400
        };
    }
}
