namespace UMS_BE.Models.DTOs;

public class PlatformAccessRequest
{
    public int PlatformId { get; set; }
    public string? ClientId { get; set; }
}

public class PlatformAccessResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool HasAccess { get; set; }
    public PlatformAccessDetails? Platform { get; set; }
    public string? PlatformSpecificToken { get; set; } // Optional: new token for this platform
}

public class PlatformAccessDetails
{
    public int PlatformId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string[] RedirectUris { get; set; } = Array.Empty<string>();
    public string[] PostLogoutRedirectUris { get; set; } = Array.Empty<string>();
    public string[] AllowedScopes { get; set; } = Array.Empty<string>();
    public List<string> Permissions { get; set; } = new();
    public List<string> Roles { get; set; } = new();
}

public class PlatformRedirectResponse
{
    public string RedirectUrl { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public PlatformAccessDetails Platform { get; set; } = null!;
}
