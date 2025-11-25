namespace UMS_BE.Models;

public class Platform
{
    public int PlatformId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RedirectUris { get; set; } = string.Empty; // JSON array as string
    public string PostLogoutRedirectUris { get; set; } = string.Empty; // JSON array as string
    public string AllowedScopes { get; set; } = string.Empty; // JSON array as string
    public bool IsActive { get; set; } = true;
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    // Navigation properties
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    public ICollection<UserPlatform> UserPlatforms { get; set; } = new List<UserPlatform>();
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}
