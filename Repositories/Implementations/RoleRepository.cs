using Microsoft.EntityFrameworkCore;
using UMS_BE.Data;
using UMS_BE.Models;
using UMS_BE.Repositories.Interfaces;

namespace UMS_BE.Repositories.Implementations;

public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public RoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(int roleId)
    {
        return await _context.Roles
            .Include(r => r.Platform)
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.RoleId == roleId);
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .Include(r => r.Platform)
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _context.Roles
            .Include(r => r.Platform)
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task<Role> CreateAsync(Role role, int? createdBy = null)
    {
        role.CreatedBy = createdBy;
        role.CreatedAt = DateTime.UtcNow;
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<Role> UpdateAsync(Role role, int? updatedBy = null)
    {
        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedBy = updatedBy;
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<bool> DeleteAsync(int roleId, int? deletedBy = null)
    {
        var role = await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.RoleId == roleId);
        if (role == null || role.DeletedAt != null) return false;

        // Soft delete
        role.DeletedAt = DateTime.UtcNow;
        role.DeletedBy = deletedBy;
        role.IsActive = false;
        
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int roleId)
    {
        return await _context.Roles.AnyAsync(r => r.RoleId == roleId);
    }

    public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
    {
        var exists = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (exists) return false;

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole == null) return false;

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .ThenInclude(p => p.Platform)
            .Select(rp => rp.Permission)
            .ToListAsync();
    }
}
