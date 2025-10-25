using UMS_BE.Models;

namespace UMS_BE.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(int roleId);
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role> CreateAsync(Role role, int? createdBy = null);
    Task<Role> UpdateAsync(Role role, int? updatedBy = null);
    Task<bool> DeleteAsync(int roleId, int? deletedBy = null);
    Task<bool> ExistsAsync(int roleId);
    Task<bool> AssignRoleToUserAsync(int userId, int roleId);
    Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
    Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId);
}
