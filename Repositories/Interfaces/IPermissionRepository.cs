using UMS_BE.Models;

namespace UMS_BE.Repositories.Interfaces;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(int permissionId);
    Task<IEnumerable<Permission>> GetAllAsync();
    Task<IEnumerable<Permission>> GetByPlatformIdAsync(int platformId);
    Task<Permission> CreateAsync(Permission permission, int? createdBy = null);
    Task<Permission> UpdateAsync(Permission permission, int? updatedBy = null);
    Task<bool> DeleteAsync(int permissionId, int? deletedBy = null);
    Task<bool> ExistsAsync(int permissionId);
    Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId);
    Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId);
}
