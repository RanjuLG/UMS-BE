namespace UMS_BE.Models;

public class Permission
{
    public int PermissionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty; // e.g., "users", "products"
    public string Action { get; set; } = string.Empty; // e.g., "create", "read", "update", "delete"
    public int PlatformId { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    // Navigation properties
    public Platform Platform { get; set; } = null!;
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
