using UMS_BE.Models;

namespace UMS_BE.Repositories.Interfaces;

public interface IPlatformRepository
{
    Task<Platform?> GetByIdAsync(int platformId);
    Task<Platform?> GetByClientIdAsync(string clientId);
    Task<IEnumerable<Platform>> GetAllAsync();
    Task<Platform> CreateAsync(Platform platform, int? createdBy = null);
    Task<Platform> UpdateAsync(Platform platform, int? updatedBy = null);
    Task<bool> DeleteAsync(int platformId, int? deletedBy = null);
    Task<bool> ExistsAsync(int platformId);
}
