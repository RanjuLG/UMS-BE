using Microsoft.EntityFrameworkCore;
using UMS_BE.Data;
using UMS_BE.Models;
using UMS_BE.Repositories.Interfaces;

namespace UMS_BE.Repositories.Implementations;

public class PlatformRepository : IPlatformRepository
{
    private readonly ApplicationDbContext _context;

    public PlatformRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Platform?> GetByIdAsync(int platformId)
    {
        return await _context.Platforms
            .Include(p => p.Permissions)
            .FirstOrDefaultAsync(p => p.PlatformId == platformId);
    }

    public async Task<Platform?> GetByClientIdAsync(string clientId)
    {
        return await _context.Platforms
            .Include(p => p.Permissions)
            .FirstOrDefaultAsync(p => p.ClientId == clientId);
    }

    public async Task<IEnumerable<Platform>> GetAllAsync()
    {
        return await _context.Platforms
            .Include(p => p.Permissions)
            .ToListAsync();
    }

    public async Task<Platform> CreateAsync(Platform platform, int? createdBy = null)
    {
        platform.CreatedBy = createdBy;
        platform.CreatedAt = DateTime.UtcNow;
        _context.Platforms.Add(platform);
        await _context.SaveChangesAsync();
        return platform;
    }

    public async Task<Platform> UpdateAsync(Platform platform, int? updatedBy = null)
    {
        platform.UpdatedAt = DateTime.UtcNow;
        platform.UpdatedBy = updatedBy;
        _context.Platforms.Update(platform);
        await _context.SaveChangesAsync();
        return platform;
    }

    public async Task<bool> DeleteAsync(int platformId, int? deletedBy = null)
    {
        var platform = await _context.Platforms.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.PlatformId == platformId);
        if (platform == null || platform.DeletedAt != null) return false;

        // Soft delete
        platform.DeletedAt = DateTime.UtcNow;
        platform.DeletedBy = deletedBy;
        platform.IsActive = false;
        
        _context.Platforms.Update(platform);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int platformId)
    {
        return await _context.Platforms.AnyAsync(p => p.PlatformId == platformId);
    }
}
