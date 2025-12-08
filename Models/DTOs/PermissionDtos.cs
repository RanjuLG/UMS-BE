namespace UMS_BE.Models.DTOs;

public class PermissionDto
{
    public int PermissionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public int PlatformId { get; set; }
    public string PlatformName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreatePermissionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public int PlatformId { get; set; }
}

public class UpdatePermissionRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Resource { get; set; }
    public string? Action { get; set; }
    public bool? IsActive { get; set; }
}

public class AssignPermissionRequest
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
}
