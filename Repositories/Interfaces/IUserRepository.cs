using UMS_BE.Models;

namespace UMS_BE.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUserNameAsync(string userName);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> CreateAsync(User user, int? createdBy = null);
    Task<User> UpdateAsync(User user, int? updatedBy = null);
    Task<bool> DeleteAsync(int userId, int? deletedBy = null);
    Task<bool> ExistsAsync(int userId);
    Task<IEnumerable<Role>> GetUserRolesAsync(int userId);
    Task<IEnumerable<Permission>> GetUserPermissionsForPlatformAsync(int userId, int platformId);
    Task<IEnumerable<Platform>> GetUserPlatformsAsync(int userId);
    Task AddUserToPlatformAsync(int userId, int platformId, int? createdBy = null);
}
