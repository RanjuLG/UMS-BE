namespace UMS_BE.Models.DTOs;

public class PlatformDto
{
    public int PlatformId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> RedirectUris { get; set; } = new();
    public List<string> PostLogoutRedirectUris { get; set; } = new();
    public List<string> AllowedScopes { get; set; } = new();
    public bool IsActive { get; set; }
}

public class CreatePlatformRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> RedirectUris { get; set; } = new();
    public List<string> PostLogoutRedirectUris { get; set; } = new();
    public List<string> AllowedScopes { get; set; } = new();
}

public class UpdatePlatformRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string>? RedirectUris { get; set; }
    public List<string>? PostLogoutRedirectUris { get; set; }
    public List<string>? AllowedScopes { get; set; }
    public bool? IsActive { get; set; }
}
