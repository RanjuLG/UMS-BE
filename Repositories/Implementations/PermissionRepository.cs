using Microsoft.EntityFrameworkCore;
using UMS_BE.Data;
using UMS_BE.Models;
using UMS_BE.Repositories.Interfaces;

namespace UMS_BE.Repositories.Implementations;

public class PermissionRepository : IPermissionRepository
{
    private readonly ApplicationDbContext _context;

    public PermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Permission?> GetByIdAsync(int permissionId)
    {
        return await _context.Permissions
            .Include(p => p.Platform)
            .FirstOrDefaultAsync(p => p.PermissionId == permissionId);
    }

    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        return await _context.Permissions
            .Include(p => p.Platform)
            .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetByPlatformIdAsync(int platformId)
    {
        return await _context.Permissions
            .Include(p => p.Platform)
            .Where(p => p.PlatformId == platformId)
            .ToListAsync();
    }

    public async Task<Permission> CreateAsync(Permission permission, int? createdBy = null)
    {
        permission.CreatedBy = createdBy;
        permission.CreatedAt = DateTime.UtcNow;
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();
        return permission;
    }

    public async Task<Permission> UpdateAsync(Permission permission, int? updatedBy = null)
    {
        permission.UpdatedAt = DateTime.UtcNow;
        permission.UpdatedBy = updatedBy;
        _context.Permissions.Update(permission);
        await _context.SaveChangesAsync();
        return permission;
    }

    public async Task<bool> DeleteAsync(int permissionId, int? deletedBy = null)
    {
        var permission = await _context.Permissions.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.PermissionId == permissionId);
        if (permission == null || permission.DeletedAt != null) return false;

        // Soft delete
        permission.DeletedAt = DateTime.UtcNow;
        permission.DeletedBy = deletedBy;
        permission.IsActive = false;
        
        _context.Permissions.Update(permission);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int permissionId)
    {
        return await _context.Permissions.AnyAsync(p => p.PermissionId == permissionId);
    }

    public async Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId)
    {
        var exists = await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (exists) return false;

        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };

        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (rolePermission == null) return false;

        _context.RolePermissions.Remove(rolePermission);
        await _context.SaveChangesAsync();
        return true;
    }
}
